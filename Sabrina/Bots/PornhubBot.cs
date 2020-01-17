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
		public List<Video> IndexedVideos = new List<Video>();

		private static string _logoUrl = @"https://di.phncdn.com/www-static/images/pornhub_logo_straight.png";
		private readonly CancellationToken _cancellationToken;
		private readonly DiscordClient _client;

		private readonly HttpClient _httpClient;
		private readonly Thread _mainThread;

		public PornhubBot(DiscordClient client, CancellationToken token)
		{
			_cancellationToken = token;
			_client = client;

			_httpClient = new HttpClient();

			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));

			_httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "71.0.3578.98"));

			_mainThread = new Thread(async () =>
			{
				try
				{
					await MainThread();
				}
				catch (TaskCanceledException)
				{ }
			});
			_mainThread.Start();
		}

		public void Dispose()
		{
			this.Dispose(true);

			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			try
			{
				_httpClient.Dispose();
				_mainThread.Interrupt();
				_mainThread.Abort();
			}
			catch (TaskCanceledException)
			{ }
			catch (PlatformNotSupportedException)
			{ }
		}

		private async Task<IEnumerable<string>> GetNewestPornhubVideoIds(string userName, CancellationToken cancellationToken)
		{
			List<string> newestVideos = new List<string>();

			try
			{
				string url = $"https://www.pornhub.com/users/{userName}/videos";
				var response = await _httpClient.GetAsync(url, cancellationToken);

				if (response.StatusCode != HttpStatusCode.OK)
				{
					return newestVideos;
				}

				var data = await response.Content.ReadAsStreamAsync();
				var doc = new HtmlDocument();
				doc.Load(data);

				var tableNode = doc.DocumentNode.Descendants("ul").FirstOrDefault(n => n.HasClass("videos") && n.HasClass("row-3-thumbs") && n.HasClass("gap-row-15"));

				if (tableNode == null)
				{
					tableNode = doc.DocumentNode.Descendants("ul").FirstOrDefault(n => n.HasClass("videos") && n.HasClass("row-5-thumbs") && n.HasClass("pornstarsVideos") && n.HasClass("search-video-thumbs"));
				}

				var allLinkNodes = tableNode.Descendants("a");

				foreach (var linkNode in allLinkNodes)
				{
					var href = linkNode.GetAttributeValue("href", string.Empty);
					if (!href.Contains("viewkey"))
					{
						continue;
					}

					string id = href.Split(new string[] { "?viewkey=" }, StringSplitOptions.RemoveEmptyEntries)[1];

					if (newestVideos.Any(v => v == id))
					{
						continue;
					}

					newestVideos.Add(id);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return newestVideos;
			}

			return newestVideos;
		}

		private async Task MainThread()
		{
			using var context = new DiscordContext();

			while (!_cancellationToken.IsCancellationRequested)
			{
				foreach (var platform in context.Joiplatform.ToArray())
				{
					foreach (var link in context.CreatorPlatformLink.ToArray())
					{
						IEnumerable<string> newestVideos = null;

						switch (platform.BaseUrl)
						{
							case "https://www.pornhub.com":
								newestVideos = await GetNewestPornhubVideoIds(link.Identification, _cancellationToken);
								break;
						}

						foreach (var newestVideo in newestVideos)
						{
							if (newestVideo == null || context.IndexedVideo.Any(iv => iv.Identification == newestVideo))
							{
								continue;
							}

							await Task.Delay(2000, _cancellationToken);
							var fullVideo = await Video.FromPornhubId(newestVideo);

							var creator = await context.Creator.FindAsync(keyValues: new object[] { link.CreatorId }, cancellationToken: _cancellationToken);
							DiscordUser discordUser = null;

							if (creator.DiscordUserId != null)
							{
								try
								{
									discordUser = await _client.GetUserAsync(Convert.ToUInt64(creator.DiscordUserId.Value));
								}
								catch (Exception)
								{ }
							}

							IndexedVideo indexedVideo = new IndexedVideo()
							{
								CreationDate = DateTime.Now,
								CreatorId = creator.Id,
								Identification = fullVideo.ID,
								Link = fullVideo.Url,
								PlatformId = platform.Id
							};

							DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
							{
								Color = DiscordColor.Gold,
								Title = $"{fullVideo.Creator} has uploaded a new video!",
								Url = fullVideo.Url,
								ImageUrl = fullVideo.ImageUrl,
								ThumbnailUrl = _logoUrl
							};

							builder.AddField("Title", fullVideo.Title);
							if (creator.DiscordUserId != null && discordUser != null)
							{
								builder.AddField("Creator", discordUser.Mention);
							}

							builder.AddField("Link", fullVideo.Url);

							var embed = builder.Build();

							foreach (var updateChannelId in context.SabrinaSettings.Where(ss => ss.ContentChannel != null).Select(ss => ss.ContentChannel))
							{
								if (_client.Guilds.Any(g => g.Value.Channels.Any(c => c.Key == Convert.ToUInt64(updateChannelId))))
								{
									var updateChannel = await _client.GetChannelAsync(Convert.ToUInt64(updateChannelId));
									await _client.SendMessageAsync(updateChannel, embed: embed);
								}
							}

							await context.IndexedVideo.AddAsync(indexedVideo, _cancellationToken);
						}

						await context.SaveChangesAsync(_cancellationToken);
						await Task.Delay(3000, _cancellationToken);
					}
					await Task.Delay(3000, _cancellationToken);
				}

				await Task.Delay(120000, _cancellationToken);
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

		private static readonly string[] MediaTypeHeaders = new[] { "text/html", "application/xhtml+xml", "application/xml", "image/webp", "image/apng" };

		public static async Task<Video> FromPornhubId(string id)
		{
			using var client = new HttpClient();

			foreach (var mediaTypeHeader in MediaTypeHeaders)
			{
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaTypeHeader));
			}
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

			const string meta = "/html/head/meta";

			var metaNodes = doc.DocumentNode.SelectNodes(meta);

			if (metaNodes == null || !metaNodes.Any())
			{
				//Refresh Cookie or something, dunno
				return null;
			}

			var titleNode = doc.DocumentNode.SelectNodes(meta).Where(e => e.Attributes["property"]?.Value == "og:title").FirstOrDefault();
			var imageNode = doc.DocumentNode.SelectNodes(meta).Where(e => e.Attributes["property"]?.Value == "og:image").FirstOrDefault();
			var videoInfoNode = doc.DocumentNode.Descendants("div").Where(d => d.HasClass("video-detailed-info")).FirstOrDefault();
			HtmlNode usernameNode = null;
			if (videoInfoNode != null)
			{
				usernameNode = videoInfoNode.Descendants("a").FirstOrDefault();
			}
			//var userName = doc.DocumentNode.Descendants("div")
			//    .Where(d => d.GetAttributeValue("class", "") == "video-detailed-info").First().Descendants("a").First()
			//    .InnerText;

			if (titleNode == null || imageNode == null || usernameNode == null)
			{
				return null;
			}

			var video = new Video
			{
				Url = $"https://www.pornhub.com/view_video.php?viewkey={id}",
				Creator = usernameNode.InnerText,
				CreationDate = DateTime.Now,
				ImageUrl = imageNode.GetAttributeValue("content", string.Empty),
				Title = titleNode.GetAttributeValue("content", string.Empty),
				ID = id
			};

			return video;
		}
	}
}