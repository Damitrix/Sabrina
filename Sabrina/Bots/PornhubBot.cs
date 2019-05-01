using Sabrina.Models;
using System.Net;
using System.Net.Http;

namespace Sabrina.Pornhub
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using HtmlAgilityPack;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    public class PornhubBot : IDisposable
    {
        public static bool Exit = false;

        public List<Video> IndexedVideos = new List<Video>();

        private readonly DiscordClient _client;

        private HttpClient _httpClient;
        private Thread _mainThread;

        public PornhubBot(DiscordClient client)
        {
            this._client = client;

            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));

            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "71.0.3578.98"));

            _mainThread = new Thread(async () => await MainThread());
            _mainThread.Start();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _mainThread.Abort();
        }

        private async Task<Video> GetNewestPornhubVideo(string userName)
        {
            Video newestVideo = null;

            try
            {
                string url = $"https://www.pornhub.com/users/{userName}/videos";
                var response = await _httpClient.GetAsync(url).ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                var data = await response.Content.ReadAsStreamAsync();
                var doc = new HtmlDocument();
                doc.Load(data);

                var node = doc.DocumentNode.SelectSingleNode(
                    "//*[@class=\"videos row-3-thumbs\"]/li[1]/div/div[1]/div/a");
                if (node == null)
                {
                    url = $"https://www.pornhub.com/model/{userName}";
                    response = await _httpClient.GetAsync(url).ConfigureAwait(false);

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return null;
                    }

                    data = await response.Content.ReadAsStreamAsync();
                    doc = new HtmlDocument();
                    doc.Load(data);

                    node = doc.DocumentNode.SelectSingleNode(
                        "//*[@class=\"videos row-5-thumbs search-video-thumbs pornstarsVideos\"]/li[1]/div[1]/div[1]/div[2]/a");
                    if (node == null)
                    {
                        return null;
                    }
                }

                string id = node.GetAttributeValue("href", string.Empty).Split(new string[] { "?viewkey=" }, StringSplitOptions.RemoveEmptyEntries)[1];
                newestVideo = await Video.FromPornhubId(id);

                if (newestVideo == null)
                {
                    return null;
                }

                if (newestVideo.Creator == null)
                {
                    newestVideo.Creator = userName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            return newestVideo;
        }

        private async Task MainThread()
        {
            while (!Exit)
            {
                var context = new DiscordContext();
                foreach (var platform in context.Joiplatform)
                {
                    foreach (var link in context.CreatorPlatformLink)
                    {
                        Video newestVideo = null;

                        switch (platform.BaseUrl)
                        {
                            case "https://www.pornhub.com":
                                newestVideo = await GetNewestPornhubVideo(link.Identification);
                                break;
                        }

                        if (newestVideo == null || context.IndexedVideo.Any(iv => iv.Identification == newestVideo.ID))
                        {
                            await Task.Delay(6000);
                            continue;
                        }

                        var creator = await context.Creator.FindAsync(link.CreatorId);
                        var discordUser = _client.GetUserAsync(Convert.ToUInt64(creator.DiscordUserId.Value));

                        IndexedVideo indexedVideo = new IndexedVideo()
                        {
                            CreationDate = DateTime.Now,
                            CreatorId = creator.Id,
                            Identification = newestVideo.ID,
                            Link = newestVideo.Url,
                            PlatformId = platform.Id
                        };

                        await context.IndexedVideo.AddAsync(indexedVideo);

                        DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Gold,
                            Title = $"{newestVideo.Creator} has uploaded a new video!",
                            Url = newestVideo.Url,
                            ThumbnailUrl = newestVideo.ImageUrl
                        };

                        builder.AddField("Title", newestVideo.Title);
                        if (creator.DiscordUserId != null)
                        {
                            builder.AddField("Creator", (await discordUser).Mention);
                        }

                        var embed = builder.Build();

                        await context.SaveChangesAsync();

                        foreach (var updateChannelId in context.SabrinaSettings.Where(ss => ss.ContentChannel != null).Select(ss => ss.ContentChannel))
                        {
                            try
                            {
                                var updateChannel = await _client.GetChannelAsync(Convert.ToUInt64(updateChannelId));

                                await _client.SendMessageAsync(updateChannel, embed: embed);
                            }
                            catch (DSharpPlus.Exceptions.UnauthorizedException)
                            {
                                // No other way, to handle it
                            }
                        }
                    }
                }
                await Task.Delay(120000);
            }
        }
    }

    public class Video
    {
        public DateTime CreationDate;

        public string Creator;

        public string ID;
        public string ImageUrl;

        public string Title;

        public string Url;

        public static async Task<Video> FromPornhubId(string id)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/apng"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));

            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "72.0.3626.119"));

            client.DefaultRequestHeaders.Connection.Add("close");

            WebResponse response = null;

            try
            {
                var request = HttpWebRequest.CreateHttp($"https://www.pornhub.com/view_video.php?viewkey={id}");
                request.Accept =
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                request.UserAgent = "Chrome/72.0.3626.119";
                request.ProtocolVersion = new Version(1, 0);
                response = await request.GetResponseAsync();
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"Couldn't get Pornhub Video ({id})").ConfigureAwait(false);
                await Console.Error.WriteLineAsync(ex.Message);
                return null;
            }

            var header = response.Headers;
            var length = response.ContentLength;

            var data = response.GetResponseStream();
            var doc = new HtmlDocument();

            if (data == null)
            {
                return null;
            }

            doc.Load(data);

            var metaNodes = doc.DocumentNode.SelectNodes("/html/head/meta");

            if (metaNodes == null || !metaNodes.Any())
            {
                //Refresh Cookie or something, dunno
                return null;
            }

            var titleNode = doc.DocumentNode.SelectNodes("/html/head/meta").Where(e => e.Attributes["property"]?.Value == "og:title").FirstOrDefault();
            var imageNode = doc.DocumentNode.SelectNodes("/html/head/meta").Where(e => e.Attributes["property"]?.Value == "og:image").FirstOrDefault();
            //var userName = doc.DocumentNode.Descendants("div")
            //    .Where(d => d.GetAttributeValue("class", "") == "video-detailed-info").First().Descendants("a").First()
            //    .InnerText;

            if (titleNode == null || imageNode == null)
            {
                return null;
            }

            var video = new Video
            {
                Url = $"https://www.pornhub.com/view_video.php?viewkey={id}",
                Creator = null,
                CreationDate = DateTime.Now,
                ImageUrl = imageNode.GetAttributeValue("content", string.Empty),
                Title = titleNode.GetAttributeValue("content", string.Empty),
                ID = id
            };

            return video;
        }
    }
}