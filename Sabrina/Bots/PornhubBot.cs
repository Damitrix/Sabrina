using System.Net;
using System.Net.Cache;
using System.Net.Http;
using Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Sabrina.Models;

namespace Sabrina.Pornhub
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using HtmlAgilityPack;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    public class PornhubBot : IDisposable
    {
        public static bool Exit = false;

        public List<Video> IndexedVideos = new List<Video>();

        private readonly DiscordClient _client;

        private Thread _mainThread;

        private HttpClient _httpClient;

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
                            await Task.Delay(3000);
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
                            var updateChannel = await _client.GetChannelAsync(Convert.ToUInt64(updateChannelId));

                            await _client.SendMessageAsync(updateChannel, embed: embed);
                        }
                    }
                }
                await Task.Delay(120000);
            }
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
                    return null;
                }

                string id = node.GetAttributeValue("href", string.Empty).Split(new string[] {"?viewkey="}, StringSplitOptions.RemoveEmptyEntries)[1];
                newestVideo = await Video.FromPornhubId(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            return newestVideo;
        }

        protected virtual void Dispose(bool disposing)
        {
            _mainThread.Abort();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class Video
    {
        public DateTime CreationDate;

        public string Creator;

        public string ImageUrl;

        public string Title;

        public string Url;

        public string ID;

        public static async Task<Video> FromPornhubId(string id)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));

            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "71.0.3578.98"));

            client.DefaultRequestHeaders.Connection.Add("close");
            

            HttpResponseMessage response = null;

            try
            {
                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://www.pornhub.com/view_video.php?viewkey={id}"));
                msg.Version = HttpVersion.Version11;

                response = await client.SendAsync(msg, HttpCompletionOption.ResponseContentRead,
                    CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"Couldn't get Pornhub Video ({id})").ConfigureAwait(false);
                await Console.Error.WriteLineAsync(ex.Message);
                return null;
            }
            
            var data = await response.Content.ReadAsStreamAsync();
            var doc = new HtmlDocument();

            if(data == null)
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
            var test = doc.DocumentNode.Descendants("div")
                .Where(d => d.GetAttributeValue("class", "") == "video-detailed-info").First();
            var userName = doc.DocumentNode.Descendants("div")
                .Where(d => d.GetAttributeValue("class", "") == "video-detailed-info").First().Descendants("a").First()
                .InnerText;

            if (titleNode == null || imageNode == null)
            {
                return null;
            }

            var video = new Video
            {
                Url = $"https://www.pornhub.com/view_video.php?viewkey={id}",
                Creator = userName,
                CreationDate = DateTime.Now,
                ImageUrl = imageNode.GetAttributeValue("content", string.Empty),
                Title = titleNode.GetAttributeValue("content", string.Empty),
                ID = id
            };

            return video;
        }
    }
}