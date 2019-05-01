using Configuration;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Sabrina.Entities;
using Sabrina.Models;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using Convert = System.Convert;
using Timer = System.Timers.Timer;

namespace Sabrina.Bots
{
    internal class SankakuBot : IDisposable
    {
        private const string _baseUrl = "https://" + _domain;
        private const string _domain = "capi-v2.sankakucomplex.com";
        private const string _indexUrl = "/posts";

        //private const string _tagUrl = "/tags";
        private readonly Dictionary<long, List<long>> _cachedImages = new Dictionary<long, List<long>>();

        private readonly DiscordClient _client;
        private readonly HttpClient _httpClient;
        private readonly ConcurrentBag<long> _postedImages = new ConcurrentBag<long>();
        private Thread _mainThread;
        private Timer _postTimer;
        private Timer _scrapeTimer;
        private Timer _updateCacheTimer;

        public SankakuBot(DiscordClient client)
        {
            _client = client;

            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri(_baseUrl), new Cookie("login", Config.SankakuLogin));
            cookieContainer.Add(new Uri(_baseUrl), new Cookie("pass_hash", Config.SankakuPassword));
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(_baseUrl)
            };

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));

            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "71.0.3578.98"));
        }

        private enum Order
        {
            Newest,
            Random
        }

        public void Dispose()
        {
            _mainThread.Abort();
            Dispose();
            GC.SuppressFinalize(this);
        }

        public void Initialize()
        {
            _mainThread = new Thread(() => MainTask().GetAwaiter().GetResult());
            _mainThread.Start();
        }

        public async Task<bool> PostRandom(DiscordChannel channel, int skip = 0)
        {
            var context = new DiscordContext();

            var channelIDLong = Convert.ToInt64(channel.Id);

            var startDate = DateTime.Now - TimeSpan.FromDays(90);

            if (!_cachedImages.ContainsKey(channelIDLong))
            {
                Console.WriteLine($"Couldn't find cached Images for Channel {channelIDLong}");
                return false;
            }

            var images = _cachedImages[Convert.ToInt64(channelIDLong)];

            var imgToPost = images.Where(cImg => !_postedImages.Contains(cImg)).Skip(skip).FirstOrDefault();

            if (imgToPost == 0)
            {
                await Console.Error.WriteLineAsync("Could not find a suitable Sankaku Image").ConfigureAwait(false);
                return false;
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.SpringGreen,
                Title = context.Puns.Skip(Helpers.RandomGenerator.RandomInt(0, context.Puns.Count() - 1)).First().Text
            };

            var imageUri = await GetOriginalImageUrl(imgToPost).ConfigureAwait(false);

            if (imageUri == null)
            {
                return false;
            }

            var link = HttpUtility.HtmlDecode(imageUri.AbsoluteUri);

            var response = await _httpClient.GetAsync(link);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                await Console.Error.WriteLineAsync($"Could not get Image Link for {imgToPost}").ConfigureAwait(false);
                return false;
            }

            var stream = await response.Content.ReadAsStreamAsync();

            Stream outStream = null;
            string extension = "";
            SKImage img = SKImage.FromEncodedData(stream);

            if (img != null)
            {
                if (img.ColorType == SKColorType.Bgra8888 || img.ColorType == SKColorType.Gray8)
                {
                    //Convert Types, that need an indexed Colortable to a non-indexed Type
                    var bitmap = new SKBitmap(new SKImageInfo(img.Width, img.Height, SKColorType.Rgba8888));
                    var canvas = new SKCanvas(bitmap);
                    canvas.DrawImage(img, 0, 0, null);
                    canvas.Flush();
                    img = SKImage.FromBitmap(bitmap);
                }

                var imageData = img.Encode(SKEncodedImageFormat.Jpeg, 75);

                outStream = imageData.AsStream();

                extension = ".jpeg";
            }
            else
            {
                return false;
            }

            if (outStream == null || outStream.Length == 0)
            {
                await Console.Error.WriteLineAsync($"Stream for SankakuImage ({imgToPost}) was 0 length or null").ConfigureAwait(false);
                return false;
            }

            try
            {
                using (outStream)
                {
                    Console.WriteLine("SankakuBot: Sending File");
                    var msgTask = channel.SendFileAsync(outStream, Helpers.GetSafeFilename(builder.Title + extension),
                        embed: builder.Build());

                    msgTask.Wait(30000);

                    DiscordMessage msg = null;

                    if (msgTask.IsCompleted)
                    {
                        msg = await msgTask;
                    }
                    else
                    {
                        Console.WriteLine($"SankakuBot: Couldn't send File with name {Helpers.GetSafeFilename(builder.Title + extension)}");
                        outStream.Close();
                        await _client.ReconnectAsync(true);
                        return false;
                    }

                    Console.WriteLine("SankakuBot: Finished Sending File");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SankakuBot: Something went wrong while trying to send file");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
            }
            finally
            {
                context.SankakuPost.Add(new SankakuPost()
                {
                    Date = DateTime.Now,
                    ImageId = imgToPost,
                    MessageId = -1
                });

                await context.SaveChangesAsync();

                _postedImages.Add(imgToPost);
            }

            return true;
        }

        private async Task Client_MessageReactionAdded(DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
        {
            var context = new DiscordContext();

            var message = await context.SankakuPost.FirstOrDefaultAsync(post => post.MessageId == Convert.ToInt64(e.Message.Id));

            if (message != null)
            {
                SankakuImageVote vote = null;

                vote = new SankakuImageVote()
                {
                    ImageId = message.ImageId,
                    UserId = Convert.ToInt64(e.User.Id),
                    VoteValue = 0
                };

                string discordName = e.Emoji.GetDiscordName();

                if (Config.Emojis.Confirms.Contains(discordName))
                {
                    vote.VoteValue = 1;
                }
                else if (Config.Emojis.Love.Contains(discordName))
                {
                    vote.VoteValue = 3;
                }
                else if (Config.Emojis.Declines.Contains(discordName))
                {
                    vote.VoteValue = -1;
                }
                else if (Config.Emojis.Hate.Contains(discordName))
                {
                    vote.VoteValue = -3;
                }

                if (vote.VoteValue != 0)
                {
                    await context.SankakuImageVote.AddAsync(vote);
                }

                await context.SaveChangesAsync();
            }
        }

        private async Task Client_MessageReactionRemoved(DSharpPlus.EventArgs.MessageReactionRemoveEventArgs e)
        {
            var context = new DiscordContext();

            var message = await context.SankakuPost.FirstOrDefaultAsync(post => post.MessageId == Convert.ToInt64(e.Message.Id));

            if (message != null)
            {
                int VoteValue = 0;

                string discordName = e.Emoji.GetDiscordName();

                if (Config.Emojis.Confirms.Contains(discordName))
                {
                    VoteValue = 1;
                }
                else if (Config.Emojis.Love.Contains(discordName))
                {
                    VoteValue = 3;
                }
                else if (Config.Emojis.Declines.Contains(discordName))
                {
                    VoteValue = -1;
                }
                else if (Config.Emojis.Hate.Contains(discordName))
                {
                    VoteValue = -3;
                }

                var vote = await context.SankakuImageVote.FirstOrDefaultAsync(sankakuVote => sankakuVote.ImageId == message.ImageId &&
                                                                                             sankakuVote.UserId == Convert.ToInt64(e.User.Id) &&
                                                                                             sankakuVote.VoteValue == VoteValue);

                if (vote != null)
                {
                    context.SankakuImageVote.Remove(vote);
                }

                await context.SaveChangesAsync();
            }
        }

        private async Task<Uri> GetOriginalImageUrl(long id)
        {
            var response = await _httpClient.GetAsync(_baseUrl + _indexUrl + $"?limit=1&page=1&tags=id_range:{id}").ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var image = Entities.Sankaku.Image.FromJson(json).FirstOrDefault();

            if (image == null)
            {
                return null;
            }

            return image.FileUrl;
        }

        private async Task MainTask()
        {
            Console.WriteLine("Sankakubot: Starting");
            if (Config.SankakuLogin == null || Config.SankakuPassword == null)
            {
                Console.WriteLine("No Login info for Sankaku provided. Skipping Initialization of SankakuBot.");
                return;
            }

            _client.MessageReactionAdded += Client_MessageReactionAdded;
            _client.MessageReactionRemoved += Client_MessageReactionRemoved;

            _scrapeTimer = new Timer(TimeSpan.FromMinutes(2).TotalMilliseconds)
            {
                AutoReset = true
            };

            _scrapeTimer.Elapsed += async (object o, ElapsedEventArgs e) => await Task.Run(async () => await Scrape());

            _scrapeTimer.Start();

            _postTimer = new Timer(TimeSpan.FromMinutes(60).TotalMilliseconds)
            {
                AutoReset = true
            };
            _postTimer.Elapsed += async (object sender, ElapsedEventArgs e) => await PostTimer_Elapsed();

            _postTimer.Start();

            _updateCacheTimer = new Timer(TimeSpan.FromMinutes(480).TotalMilliseconds)
            {
                AutoReset = true
            };
            _updateCacheTimer.Elapsed += async (object sender, ElapsedEventArgs e) => await UpdateCache();

            _updateCacheTimer.Start();

            await UpdateCache();

            DiscordContext context = new DiscordContext();

            if (!context.SankakuPost.Any() || context.SankakuPost.OrderByDescending(sp => sp.Date).First().Date < DateTime.Now - TimeSpan.FromMinutes(20))
            {
                await PostTimer_Elapsed();
            }
        }

        private async Task PostTimer_Elapsed()
        {
            DiscordContext context = new DiscordContext();

            foreach (var channelId in context.SabrinaSettings.Select(ss => ss.FeetChannel).Where(cId => cId != null))
            {
                DiscordChannel channel = null;

                if (_client.Guilds.Any(g => g.Value.Channels.Any(c => c.Id == Convert.ToUInt64(channelId))))
                {
                    channel = await _client.GetChannelAsync(Convert.ToUInt64(channelId));

                    try
                    {
                        await PostRandom(channel);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error in Sankakubot PostRandom");
                        Console.WriteLine(e);
                    }
                }
            }
        }

        private async Task<bool> Scrape(Order order = Order.Random, int limit = 100)
        {
            Console.WriteLine("SankakuBot: scraping");
            var context = new DiscordContext();

            string orderString = "random";

            switch (order)
            {
                case Order.Random:
                    orderString = "random";
                    break;

                case Order.Newest:
                    orderString = "newest";
                    break;
            }

            var response = await _httpClient.GetAsync(_baseUrl + _indexUrl + $"?limit={limit}&tags=order:{orderString}+feet").ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();

            var images = Entities.Sankaku.Image.FromJson(json);

            var allTags = context.SankakuTag.ToList();

            foreach (Entities.Sankaku.Image image in images)
            {
                if (context.SankakuImage.Find(image.Id) != null)
                {
                    continue;
                }

                SankakuImage dbImage = new SankakuImage()
                {
                    Id = image.Id,
                    Rating = (int)image.Rating,
                    RatingCount = Convert.ToInt32(image.VoteCount),
                    Score = Convert.ToInt32(image.TotalScore)
                };

                foreach (var tag in image.Tags)
                {
                    var dbTag = allTags.FirstOrDefault(savedTag => savedTag.Id == Convert.ToInt32(tag.Id));

                    if (dbTag == null)
                    {
                        dbTag = new SankakuTag()
                        {
                            Name = tag.Name,
                            Id = Convert.ToInt32(tag.Id)
                        };

                        context.SankakuTag.Add(dbTag);
                        allTags.Add(dbTag);
                    }

                    SankakuImageTag imageTag = new SankakuImageTag()
                    {
                        TagId = dbTag.Id
                    };

                    dbImage.SankakuImageTag.Add(imageTag);
                }

                await context.SankakuImage.AddAsync(dbImage).ConfigureAwait(false);
            }

            await context.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }

        private async Task UpdateCache()
        {
            Stopwatch sw = new Stopwatch();
            Console.WriteLine("Updating Sankaku Cache");
            _postedImages.Clear();
            _cachedImages.Clear();

            var context = new DiscordContext();

            sw.Start();

            var minTime = DateTime.Now - TimeSpan.FromDays(90);

            Thread.CurrentThread.Name = "SankakuBot";

            foreach (var setting in context.SabrinaSettings.Where(ss => ss.FeetChannel != null))
            {
                var channel = setting.FeetChannel.Value;

                var whiteListedTags = context.SankakuTagWhiteList.Where(whiteTag => whiteTag.ChannelId == channel);

                var blacklistedTags = context.SankakuTagBlacklist.Where(blackTag => blackTag.ChannelId == channel);

                // Note: Part is commented out because of performance problems in LINQ to SQL. Normal SQL Query behaves much better
                //var filteredImages = from image in context.SankakuImage
                //                     where
                //                        (from si in context.SankakuImageTag
                //                         join wt in context.SankakuTagWhiteList
                //                             on si.TagId equals wt.TagId
                //                         where si.ImageId == image.Id
                //                         && wt.ChannelId == channel
                //                         select wt).Sum(cwT => cwT.Weight) > 29
                //                        &&
                //                        (from si in context.SankakuImageTag
                //                         join wt in context.SankakuTagBlacklist
                //                             on si.TagId equals wt.TagId
                //                         where si.ImageId == image.Id
                //                         && wt.ChannelId == channel
                //                         select wt).Count() == 0
                //                        &&
                //                        (
                //                        (from postedImg in context.SankakuPost
                //                         where postedImg.ImageId == image.Id
                //                         select postedImg).Count() == 0
                //                        ||
                //                        (from postedImg in context.SankakuPost
                //                         where postedImg.ImageId == image.Id
                //                         orderby postedImg.Date descending
                //                         select postedImg.Date).First() < minTime
                //                        )
                //                        && image.Score > minScore
                //                     select image;

                var filteredImages = context.SankakuImage.FromSql(@"
                                                                    SELECT TOP(100) *
                                                              FROM [Sankaku.Image] mainImg
                                                              WHERE
                                                               (SELECT SUM(Weight) FROM [Sankaku.ImageTag]
                                                               JOIN [Sankaku.TagWhiteList] on [Sankaku.TagWhiteList].TagID = [Sankaku.ImageTag].TagID
                                                               WHERE [Sankaku.ImageTag].ImageID = mainImg.ID AND [Sankaku.TagWhiteList].ChannelID = 448417831067975680) > 29
                                                              AND
                                                               (SELECT Count(*) FROM [Sankaku.ImageTag]
                                                               JOIN [Sankaku.TagBlacklist] on [Sankaku.TagBlacklist].TagID = [Sankaku.ImageTag].TagID
                                                               WHERE [Sankaku.ImageTag].ImageID = mainImg.ID AND [Sankaku.TagBlacklist].ChannelID = 448417831067975680) = 0
                                                              AND
                                                               ((SELECT COUNT(1) Date FROM [Sankaku.Post]
                                                               WHERE [Sankaku.Post].ImageID = mainImg.ID) = 0
                                                               OR
                                                               (SELECT TOP(1) Date FROM [Sankaku.Post]
                                                               WHERE [Sankaku.Post].ImageID = mainImg.ID
                                                               ORDER BY Date DESC) < '2018-01-01')
                                                              AND
                                                               mainImg.Score > 60");

                var cachedImages = await filteredImages.ToListAsync();

                _cachedImages.Add(channel, cachedImages.Select(img => img.Id).ToList());
            }

            context.Dispose();
            Console.WriteLine($"Finished updating Sankaku Cache after {sw.Elapsed}");
        }
    }
}