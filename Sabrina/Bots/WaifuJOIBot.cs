using DSharpPlus;
using DSharpPlus.Entities;
using Sabrina.Entities;
using Sabrina.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using WaifuJoi.Shared.Models;
using Creator = WaifuJoi.Shared.Models.Creator;
using Timer = System.Timers.Timer;

namespace Sabrina.Bots
{
    internal class WaifuJOIBot
    {
        private const string BaseAddress = "https://waifujoi.app";
        private static readonly string AlbumAddress = BaseAddress + "/api/album";
        private static readonly string ContentAddress = BaseAddress + "/api/content";
        private static readonly string CreatorsAddress = BaseAddress + "/api/creators";
        private static readonly string ImageAddress = ContentAddress + "/image";
        private static readonly string ThumbnailAddress = ContentAddress + "/thumbnail";
        private readonly Dictionary<long, List<Content>> _cachedImages = new Dictionary<long, List<Content>>();
        private readonly DiscordClient _client;

        private Timer _postTimer;

        //private Timer _scrapeTimer;
        private Timer _updateCacheTimer;

        public WaifuJOIBot(DiscordClient client)
        {
            _client = client;
        }

        public static string GetCreatorUrl(int id)
        {
            return CreatorsAddress + "/" + id;
        }

        public static string GetImageUrl(string id)
        {
            return ImageAddress + "/" + id;
        }

        public Content GetRandomPicture(long? channelId = null)
        {
            DiscordContext context = new DiscordContext();
            Content imageToPost = null;
            var time = DateTime.Now - TimeSpan.FromDays(90);
            var posts = context.WaifuJoiContentPost.Where(post => post.Time > time).OrderBy(p => p.Time).ToList();

            if (channelId != null)
            {
                if (!_cachedImages.ContainsKey(channelId.Value))
                {
                    return null;
                }

                var viableImages = _cachedImages[channelId.Value].Where(img => posts.All(post => post.ContentId != img.Id));

                imageToPost = viableImages.Skip(Helpers.RandomGenerator.RandomInt(0, viableImages.Count() - 1)).FirstOrDefault();

                if (imageToPost == null)
                {
                    var groupedPosts = posts.GroupBy(p => p.ContentId).OrderBy(gp => gp.OrderByDescending(g => g.Time).First().Time);

                    // Get longest non-posted
                    foreach (var post in groupedPosts)
                    {
                        imageToPost = _cachedImages[channelId.Value].FirstOrDefault(cImg => cImg.Id == post.First().ContentId);
                        if (imageToPost != null)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                var viableImages = _cachedImages.First().Value.Where(img => posts.All(post => post.ContentId != img.Id));

                imageToPost = viableImages.Skip(Helpers.RandomGenerator.RandomInt(0, viableImages.Count() - 1)).FirstOrDefault();

                if (imageToPost == null)
                {
                    var groupedPosts = posts.GroupBy(p => p.ContentId).OrderBy(gp => gp.OrderByDescending(g => g.Time).First().Time);

                    // Get longest non-posted
                    foreach (var post in groupedPosts)
                    {
                        imageToPost = _cachedImages.First().Value.FirstOrDefault(cImg => cImg.Id == post.First().ContentId);
                        if (imageToPost != null)
                        {
                            break;
                        }
                    }
                }
            }

            return imageToPost;
        }

        public async Task PostRandom(DiscordChannel channel)
        {
            Console.WriteLine($"Waifubot: PostRandom {channel.Id}");
            DiscordContext context = new DiscordContext();

            var channelId = Convert.ToInt64(channel.Id);

            var imageToPost = GetRandomPicture(channelId);

            if (imageToPost == null)
            {
                return;
            }

            HttpClient client = new HttpClient();
            var response = await client.GetAsync(CreatorsAddress + "/" + imageToPost.CreatorId);
            var creator = await MessagePack.MessagePackSerializer.DeserializeAsync<Creator>(
                await response.Content.ReadAsStreamAsync());

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    Name = creator.Name,
                    IconUrl = ThumbnailAddress + "/" + creator.Avatar,
                    Url = BaseAddress + "/profile/" + creator.Id
                },
                Color = new DiscordColor("#cf5ed4"),
                Title = context.Puns.Skip(Helpers.RandomGenerator.RandomInt(0, context.Puns.Count() - 1)).First()
                    .Text,
                ImageUrl = GetImageUrl(imageToPost.Id)
            };
            Console.WriteLine("WaifuBot: Sending embed");
            await channel.SendMessageAsync(embed: builder.Build());
            Console.WriteLine("Waifubot: Sending embed finished");
            await context.WaifuJoiContentPost.AddAsync(new WaifuJoiContentPost()
            {
                ContentId = imageToPost.Id,
                Time = DateTime.Now
            });

            await context.SaveChangesAsync();
        }

        public Task Start()
        {
            Thread mainThread = new Thread(async () =>
            {
                _postTimer = new Timer(TimeSpan.FromMinutes(60).TotalMilliseconds)
                {
                    AutoReset = true
                };
                _postTimer.Elapsed += async (object sender, ElapsedEventArgs e) => await PostToAll();

                _postTimer.Start();

                _updateCacheTimer = new Timer(TimeSpan.FromMinutes(480).TotalMilliseconds)
                {
                    AutoReset = true
                };
                _updateCacheTimer.Elapsed += async (object sender, ElapsedEventArgs e) => await RefreshCache();

                await RefreshCache();
            });

            mainThread.Start();

            return Task.CompletedTask;
        }

        private async Task PostToAll()
        {
            Console.WriteLine("WaifuBot: Sending to all");
            DiscordContext context = new DiscordContext();

            var channels = context.SabrinaSettings.Where(ss => ss.FeetChannel != null).Select(setting => setting.FeetChannel.Value);

            foreach (var channelId in channels)
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
                        Console.WriteLine("Error in WaifuJOIBot PostRandom");
                        Console.WriteLine(e);
                    }
                }
            }
        }

        private async Task RefreshCache()
        {
            Console.WriteLine("WaifuBot: Refreshing Cache");
            _cachedImages.Clear();

            DiscordContext context = new DiscordContext();

            var groupedChannels = context.WaifuJoiAlbum.GroupBy(album => album.ChannelId);
            Random rnd = new Random();

            HttpClient client = new HttpClient();

            foreach (var group in groupedChannels)
            {
                List<Content> pictures = new List<Content>();

                foreach (var album in group)
                {
                    var response = await client.GetAsync(AlbumAddress + "/" + album.ContentId);

                    var model = await MessagePack.MessagePackSerializer.DeserializeAsync<ModelArray>(
                        await response.Content.ReadAsStreamAsync());

                    pictures.AddRange(model.Contents);
                }

                _cachedImages.Add(group.Key, pictures);
            }
        }
    }
}