// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TumblrBot.cs" company="SalemsTools">
//   Do whatever
// </copyright>
// <summary>
//   The tumblr bot.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Timers;
using Configuration;

namespace Sabrina.Bots
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using Sabrina.Entities.TumblrBlog;
    using Sabrina.Entities.TumblrPost;
    using Sabrina.Models;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    ///     The tumblr bot.
    /// </summary>
    internal class TumblrBot
    {
        private readonly DiscordClient _client;
        private Timer _postTimer;

        public TumblrBot(DiscordClient client)
        {
            _client = client;
        }

        public async Task InitializeAsync()
        {
            _postTimer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds)
            {
                AutoReset = true
            };
            _postTimer.Elapsed += _postTimer_Elapsed;
            _postTimer.Start();

            await Task.Run(async () => await UpdateDatabase());
        }

        public static async Task PostRandom(DiscordClient client, DiscordContext context, DiscordChannel[] channels)
        {
            TumblrPosts post = null;
            int cDays = 30;
            Random rnd = new Random();

            while (post == null)
            {
                var minDateTime = DateTime.Now - TimeSpan.FromDays(cDays);
                var validPosts = context.TumblrPosts.Where(tPost => (tPost.LastPosted == null || tPost.LastPosted < minDateTime) && tPost.IsLoli < 1);
                var count = validPosts.Count();

                if (count == 0)
                {
                    cDays--;
                    continue;
                }
                foreach (var channel in channels)
                {
                    var rndInt = rnd.Next(count);
                    post = validPosts.Skip(rndInt).First();

                    var tumblrPost = GetTumblrPostById(post.TumblrId);

                    if(tumblrPost == null)
                    {
                        post = null;
                        continue;
                    }

                    var builder = new DiscordEmbedBuilder()
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor()
                        {
                            Name = "YourAnimeAddiction"
                        },
                        Color = DiscordColor.Orange,
                        Footer = new DiscordEmbedBuilder.EmbedFooter()
                        {
                            Text = post.TumblrId.ToString()
                        },
                        ImageUrl = tumblrPost.Response.Posts[0].Photos.First().AltSizes.OrderByDescending(size => size.Height).First().Url,
                        Title = context.Puns.Skip(new Random().Next(context.Puns.Count() - 1)).First().Text
                    };

                    post.LastPosted = DateTime.Now;

                    await context.SaveChangesAsync();
                    await channel.SendMessageAsync(embed: builder.Build());
                }
            }
        }

        public async Task CheckLoli(MessageReactionAddEventArgs e)
        {
            var name = e.Emoji.GetDiscordName();
            if (name != Config.Emojis.Underage)
            {
                return;
            }

            var msg = await e.Client.Guilds.First(g => g.Key == e.Message.Channel.GuildId).Value.GetChannel(e.Message.ChannelId)
                .GetMessageAsync(e.Message.Id);
            if(msg.Embeds.Count != 1)
            {
                return;
            }

            bool isParceville = ulong.TryParse(msg.Embeds[0].Footer.Text, out ulong id);

            if (!isParceville)
            {
                return;
            }

            DiscordContext context = new DiscordContext();

            var post = await context.TumblrPosts.FindAsync(Convert.ToInt64(id));
            post.IsLoli = 1;
            await context.SaveChangesAsync();

            await msg.DeleteAsync(":underage:");
        }

        /// <summary>
        ///     Returns count of all Posts
        /// </summary>
        /// <returns>
        ///     A Count of all Posts.
        /// </returns>
        private static int GetPostCount()
        {
            string json = string.Empty;
            var url = @"http://api.tumblr.com/v2/blog/deliciousanimefeet.tumblr.com/info";
            url += "?api_key=uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("api_key", "uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki");
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }

            TumblrBlog blog = TumblrBlog.FromJson(json);
            return Convert.ToInt32(blog.Response.Blog.Posts);
        }

        /// <summary>
        /// Get's a specific Tumblr Post
        /// </summary>
        /// <param name="id">The ID of the Tumblr Post</param>
        /// <returns>
        ///     The <see cref="TumblrPost" />.
        /// </returns>
        private static TumblrPost GetTumblrPostById(long id)
        {
            string json = string.Empty;
            var url = @"http://api.tumblr.com/v2/blog/deliciousanimefeet.tumblr.com/posts";
            url += $"?id={id}";
            url += "&api_key=uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("api_key", "uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki");
            request.AutomaticDecompression = DecompressionMethods.GZip;

            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
             catch(Exception)
            {

            }
                
            if(response  == null)
            {
                return null;
            }

            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }

            return TumblrPost.FromJson(json);
        }

        /// <summary>
        ///     Gets a specific Tumblr post
        /// </summary>
        /// <param name="offset">
        ///     The offset of the post
        /// </param>
        /// <returns>
        ///     The <see cref="TumblrPost" />.
        /// </returns>
        private static async Task<TumblrPost> GetTumblrPostsByOffset(int offset)
        {
            string json = string.Empty;
            var url = @"http://api.tumblr.com/v2/blog/deliciousanimefeet.tumblr.com/posts/photo";
            url += "?api_key=uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki";
            url += "&limit=20";
            url += $"&offset={offset}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("api_key", "uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki");
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                json = await reader.ReadToEndAsync();
            }

            return TumblrPost.FromJson(json);
        }

        private async void _postTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var context = new DiscordContext();

                var lastPost = context.SabrinaSettings.First().LastTumblrPost;
                if (lastPost != null && lastPost > DateTime.Now - TimeSpan.FromHours(2))
                {
                    return;
                }

                await PostRandom();

                context.SabrinaSettings.First().LastTumblrPost = DateTime.Now;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync("Error in TumblrBot").ConfigureAwait(false);
                await Console.Error.WriteLineAsync(ex.Message).ConfigureAwait(false);
            }
        }

        private async Task PostRandom()
        {
            var context = new DiscordContext();

            var channels = context.SabrinaSettings.Where(ss => ss.FeetChannel != null).AsEnumerable().Select(async ss => await _client.GetChannelAsync(Convert.ToUInt64(ss.FeetChannel))).ToArray();
            Task.WaitAll(channels);
            await PostRandom(_client, context, channels.Select(t => t.Result).ToArray());
        }

        private async Task UpdateDatabase()
        {
            var context = new DiscordContext();

            DateTime? lastUpdate = DateTime.Now;
            try
            {
                lastUpdate = context.SabrinaSettings.First().LastTumblrUpdate;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var minDate = DateTime.Now - TimeSpan.FromDays(3);
            if (lastUpdate != null && lastUpdate > minDate)
            {
                return;
            }

            int offset = 0;
            var totalPostCount = GetPostCount();

            context.TumblrPosts.RemoveRange(context.TumblrPosts);

            await context.SaveChangesAsync();

            while (offset < totalPostCount)
            {
                Console.WriteLine($"Indexing at Position {offset} from {totalPostCount}]");
                var posts = await GetTumblrPostsByOffset(offset);

                if (posts.Response.TotalPosts != totalPostCount)
                {
                    totalPostCount = Convert.ToInt32(posts.Response.TotalPosts);
                }

                foreach (var post in posts.Response.Posts)
                {
                    if (!context.TumblrPosts.Any(tPost => tPost.TumblrId == post.Id))
                    {
                        await context.TumblrPosts.AddAsync(new TumblrPosts()
                        {
                            TumblrId = post.Id,
                            IsLoli = -1,
                            LastPosted = null
                        });
                    }
                }

                offset += posts.Response.Posts.Length;
            }

            context.SabrinaSettings.First().LastTumblrUpdate = DateTime.Now;
            await context.SaveChangesAsync();
        }
    }
}