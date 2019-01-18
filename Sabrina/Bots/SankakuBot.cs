using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Timers;
using System.Threading.Tasks;
using Sabrina.Models;
using System.IO;
using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Configuration;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using Convert = System.Convert;
using Microsoft.ML;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Trainers.FastTree.Internal;
using Timer = System.Timers.Timer;
using System.Threading;
using System.Diagnostics;
using Sabrina.Entities;
using Sabrina.Entities.Sankaku;

namespace Sabrina.Bots
{
    class SankakuBot : IDisposable
    {
        private const string _domain = "capi-v2.sankakucomplex.com";
        private const string _baseUrl = "https://" + _domain;
        private const string _indexUrl = "/posts";
        private const string _tagUrl =  "/tags";
        private DiscordClient _client;
        private Timer _scrapeTimer;
        private Timer _postTimer;

        private HttpClient _httpClient;

        private Thread _mainThread;

        public SankakuBot(DiscordClient client)
        {
            _client = client;

            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri(_baseUrl), new Cookie("login", Config.SankakuLogin));
            cookieContainer.Add(new Uri(_baseUrl), new Cookie("pass_hash", Config.SankakuPassword));
            var handler = new HttpClientHandler() {CookieContainer = cookieContainer};
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(_baseUrl)
            };

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));

            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "71.0.3578.98"));
        }

        private async Task MainTask()
        {
            if (Config.SankakuLogin == null || Config.SankakuPassword == null)
            {
                Console.WriteLine("No Login info for Sankaku provided. Skipping Initialization of SankakuBot.");
                return;
            }

            _client.MessageReactionAdded += Client_MessageReactionAdded;
            _client.MessageReactionRemoved += Client_MessageReactionRemoved;

            var scrapingTask = Task.Run(async () => await Scrape());

            _scrapeTimer = new Timer(TimeSpan.FromMinutes(15).TotalMilliseconds)
            {
                AutoReset = true
            };

            _scrapeTimer.Elapsed += async (object o, ElapsedEventArgs e) => await Task.Run(async () => await Scrape().ConfigureAwait(false));

            _scrapeTimer.Start();

            _postTimer = new Timer(TimeSpan.FromMinutes(60).TotalMilliseconds)
            {
                AutoReset = true
            };
            _postTimer.Elapsed += async (object sender, ElapsedEventArgs e) => await _postTimer_Elapsed();

            _postTimer.Start();

            DiscordContext context = new DiscordContext();

            if (!context.SankakuPost.Any() || context.SankakuPost.OrderByDescending(sp => sp.Date).First().Date < DateTime.Now - TimeSpan.FromMinutes(20))
            {
                await _postTimer_Elapsed();
            }
        }

        public void Initialize()
        {
            _mainThread = new Thread(async () => await MainTask());
            _mainThread.Start();
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

        private async Task _postTimer_Elapsed()
        {
            DiscordContext context = new DiscordContext();

            foreach (var channelId in context.SabrinaSettings.Select(ss => ss.FeetChannel).Where(cId => cId != null))
            {
                var channel = await _client.GetChannelAsync(Convert.ToUInt64(channelId));

                try
                {
                    await Task.Run(async () => await PostRandom(channel).ConfigureAwait(false));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in Sankakubot PostRandom");
                    Console.WriteLine(e);
                }
            }
        }

        private enum Order
        {
            Newest,
            Random
        }

        private async Task<bool> Scrape(Order order = Order.Random, int limit = 100)
        {
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
                    Rating = (int) image.Rating,
                    RatingCount = Convert.ToInt32(image.VoteCount),
                    Score = image.TotalScore
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

        public async Task<bool> PostRandom(DiscordChannel channel, int skip = 0)
        {
            var sw = new Stopwatch();
            sw.Start();
            var context = new DiscordContext();

            SankakuImage imgToPost = null;

            var channelIDLong = Convert.ToInt64(channel.Id);

            var blacklistedTags =
                context.SankakuTagBlacklist.Where(blackTag => blackTag.ChannelId == channelIDLong).Select(tag => tag.TagId);

            var whiteListedTags = context.SankakuTagWhitelist
                .Where(whiteTag => whiteTag.ChannelId == channelIDLong).Select(tag => new { tag.TagId, tag.Weight});

            var startDate = DateTime.Now - TimeSpan.FromDays(90);

            var viableImages = context.SankakuImage.Where(si => si.SankakuPost.Count == 0 && si.Score > 60);

            List<int> sums = new List<int>();

            var test = sw.Elapsed;

            while (imgToPost == null)
            {
                viableImages = viableImages.Where(si =>
                    !si.SankakuImageTag.Any(tag => blacklistedTags.Contains(tag.TagId)));

                viableImages = viableImages.Where(viableImage =>
                    whiteListedTags.Where(wlt => viableImage.SankakuImageTag.Any(sit => sit.TagId == wlt.TagId))
                        .Select(wlt => wlt.Weight).Sum() > 30);

                var firstNotPostedPicture = viableImages.Skip(skip).FirstOrDefault();

                if (firstNotPostedPicture == null)
                {
                    var oldestPostedPicture = viableImages.Where(si =>
                        si.SankakuPost.OrderByDescending(post => post.Date).First().Date < startDate)
                        .Skip(skip);
                }
                else
                {
                    imgToPost = firstNotPostedPicture;
                }

                if (imgToPost == null)
                {
                    await Console.Error.WriteLineAsync("Could not find a suitable Sankau Image").ConfigureAwait(false);
                    return false;
                }
            }

            var test3 = sw.Elapsed;

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.SpringGreen,
                Title = context.Puns.Skip(Helpers.RandomGenerator.RandomInt(0, context.Puns.Count() - 1)).First().Text
            };

            var imageUri = await GetOriginalImageUrl(imgToPost.Id).ConfigureAwait(false);

            if (imageUri == null)
            {
                return false;
            }

            var link = HttpUtility.HtmlDecode(imageUri.AbsoluteUri);

            var response = await _httpClient.GetAsync(link);

            var test4 = sw.Elapsed;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                await Console.Error.WriteLineAsync($"Could not get Image Link for {imgToPost.Id}").ConfigureAwait(false);
                return false;
            }

            var stream = await response.Content.ReadAsStreamAsync();

            Stream outStream = null;
            string extension = "";
            SKImage img = SKImage.FromEncodedData(stream);

            if(img != null)
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
                await Console.Error.WriteLineAsync($"Stream for SankakuImage ({imgToPost.Id}) was 0 length or null").ConfigureAwait(false);
                return false;
            }

            using (outStream)
            {
                var msg = await channel.SendFileAsync(outStream, builder.Title + extension,
                    embed: builder.Build());

                context.SankakuPost.Add(new SankakuPost()
                {
                    Date = DateTime.Now,
                    Image = imgToPost,
                    MessageId = Convert.ToInt64(msg.Id)
                });

                await context.SaveChangesAsync();
            }

            try
            {
                outStream.Close();
            }
            catch(Exception)
            {

            }

            return true;
        }
        
        public async Task<bool> PostPrediction(long userId, DiscordChannel channel)
        {
            var context = new DiscordContext();

            SankakuImage imgToPost = null;

            Random rnd = new Random();

            var channelIDLong = Convert.ToInt64(channel.Id);

            var blacklistedTags =
                await context.SankakuTagBlacklist.Where(blackTag => blackTag.ChannelId == channelIDLong).Select(tag => tag.TagId).ToListAsync();

            var startDate = DateTime.Now - TimeSpan.FromDays(90);

            var trainedModel = Train(userId);

            int skip = 0;

            while (imgToPost == null)
            {
                //imgToPost = context.SankakuImage.Where(si => si.SankakuPost.Count == 0).Skip(skip).First();

                imgToPost = context.SankakuImage.Find(88415L);

                if (imgToPost == null)
                {
                    return false;
                }

                List<MLSankakuPost> posts = new List<MLSankakuPost>();

                MLSankakuPost mlPost = new MLSankakuPost()
                {
                    //imgRating = imgToPost.Rating,
                    //score = Convert.ToSingle(imgToPost.Score),
                    //Tags = context.SankakuImageTag.Where(imageTag => imageTag.ImageId == imgToPost.Id).Select(tag => Convert.ToSingle(tag.TagId)).ToArray(),
                    Tags = 1,
                };

                posts.Add(mlPost);

                var prediction = trainedModel.Predict(posts, true);

                if (prediction.First().Score < 1)
                {
                    skip++;
                    imgToPost = null;
                }

                if(imgToPost == null)
                {
                    await Task.Delay(3000);
                }
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Orange,
                Title = context.Puns.Skip(new Random().Next(context.Puns.Count() - 1)).First().Text
            };

            var imageUri = await GetOriginalImageUrl(imgToPost.Id).ConfigureAwait(false);

            if (imageUri == null)
            {
                return false;
            }

            var link = HttpUtility.HtmlDecode(imageUri.AbsoluteUri);

            var response = await _httpClient.GetAsync(link);

            SKImage img = SKImage.FromEncodedData(await response.Content.ReadAsStreamAsync());

            if (img == null)
            {
                return false;
            }

            if (img.ColorType == SKColorType.Bgra8888 || img.ColorType == SKColorType.Gray8)
            {
                //Bgra8888 needs an indexed Colortable, so convert it to Rgba8888 first
                var bitmap = new SKBitmap(new SKImageInfo(img.Width, img.Height, SKColorType.Rgba8888));
                var canvas = new SKCanvas(bitmap);
                canvas.DrawImage(img, 0, 0, null);
                canvas.Flush();
                img = SKImage.FromBitmap(bitmap);
            }

            var imageData = img.Encode(SKEncodedImageFormat.Jpeg, 75); //Encode in Jpeg instead of Webp because of IOS

            using (var outStream = imageData.AsStream())
            {
                var msg = await channel.SendFileAsync(outStream, builder.Title + ".jpeg",
                    embed: builder.Build());

                context.SankakuPost.Add(new Models.SankakuPost()
                {
                    Date = DateTime.Now,
                    Image = imgToPost,
                    MessageId = Convert.ToInt64(msg.Id)
                });

                await context.SaveChangesAsync();
            }

            return true;
        }

        private static BatchPredictionEngine<MLSankakuPost, MLSankakuPostLikeagePrediciton> Train(long userId)
        {
            var context = new DiscordContext();

            MLContext mlContext = new MLContext(seed: 0);
            
            var data = GetPosts(userId);

            var schemaDef = SchemaDefinition.Create(typeof(MLSankakuPost));
            
            var trainData = mlContext.CreateStreamingDataView<MLSankakuPost>(data, schemaDef);

            var pipeline = mlContext.Regression.Trainers.FastTree("Label", "Features", numLeaves: 50, numTrees: 50, minDatapointsInLeaves: 20);
            
            var model = pipeline.Fit(trainData);

            return mlContext.CreateBatchPredictionEngine<MLSankakuPost, MLSankakuPostLikeagePrediciton>(trainData, true,
                schemaDef);

            //return model.MakePredictionFunction<MLSankakuPost, MLSankakuPost>(mlContext);
        }

        private static IEnumerable<MLSankakuPost> GetPosts(long userId)
        {
            var outPosts = new List<MLSankakuPost>();
            var context = new DiscordContext();

            var allTags = context.SankakuTag.ToArray();

            outPosts.Add(new MLSankakuPost()
            {
                Tags = 0,
                UserScore = 0
            });

            outPosts.Add(new MLSankakuPost()
            {
                Tags = 1,
                UserScore = 1
            });

            outPosts.Add(new MLSankakuPost()
            {
                Tags = 1,
                UserScore = 1
            });

            outPosts.Add(new MLSankakuPost()
            {
                Tags = 0,
                UserScore = 0
            });

            return outPosts;

#pragma warning disable CS0162 // Unreachable code detected
            foreach (var vote in context.SankakuImageVote.Where(indVote => indVote.UserId == userId))
#pragma warning restore CS0162 // Unreachable code detected
            {
                MLSankakuPost post = new MLSankakuPost();

                var tags = context.SankakuImageTag.Where(imageTag => imageTag.ImageId == vote.ImageId)
                    .Select(tag => tag.TagId).ToArray();

                var image = context.SankakuImage.Find(vote.ImageId);

                //post.Tags = new float[allTags.Length];

                foreach (var generalTag in allTags)
                {
                    if (tags.Contains(generalTag.Id))
                    {
                        //post.Tags[cIndex] = 1;
                    }
                    else
                    {
                        //post.Tags[cIndex] = 0;
                    }
                }
                
                post.UserScore = vote.VoteValue;
                post.imgRating = vote.Image.Rating;
                post.score = Convert.ToSingle(image.Score);


                outPosts.Add(post);
            }

            return outPosts;
        }

        public void Dispose()
        {
            _mainThread.Abort();
            Dispose();
            GC.SuppressFinalize(this);
        }

        public class MLSankakuPost
        {
            public int Tags;
            public float score;
            public float imgRating;
            [ColumnName("Label")]
            public float UserScore;

            public MLSankakuPost()
            {

            }
        }

        public class MLSankakuPostLikeagePrediciton
        {
            [ColumnName("Score")]
            public float Score;
        }
    }
}
