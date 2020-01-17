using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
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
using Timer = System.Timers.Timer;

namespace Sabrina.Bots
{
	public class WaifuJOIBot : IDisposable
	{
		private const string BaseAddress = "https://waifujoi.app/";

		//private const string BaseAddress = "http://localhost:5000/";
		private readonly Dictionary<long, List<Content>> _cachedImages = new Dictionary<long, List<Content>>();

		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private readonly DiscordClient _client;
		private readonly HttpClient _httpClient = new HttpClient();
		private Timer _announceTimer;
		private Timer _postTimer;
		private Timer _updateCacheTimer;
		private bool disposed = false;

		public WaifuJOIBot(DiscordClient client)
		{
			_client = client;
		}

		public Thread MainThread { get; private set; }

		public static string GetCreatorUrl(int id)
		{
			return BaseAddress
				   + WaifuJoi.Shared.Features.User.GetUserRequest.Route
				   + "?id="
				   + id;
		}

		public static string GetImageUrl(string id)
		{
			return BaseAddress + "api/content/image/" + id;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public async Task<Content> GetRandomPicture(long? channelId = null, CancellationToken cancellationToken = default)
		{
			using DiscordContext context = new DiscordContext();
			Content imageToPost = null;
			var time = DateTime.Now - TimeSpan.FromDays(90);
			var posts = (await context.WaifuJoiContentPost.Where(post => post.Time > time).OrderBy(p => p.Time).ToArrayAsync(cancellationToken)).GroupBy(p => p.ContentId).ToDictionary(p => p.Key, p => p.Max(s => s.Time));

			if (channelId != null)
			{
				if (!_cachedImages.ContainsKey(channelId.Value) || _cachedImages[channelId.Value].Count == 0)
				{
					return null;
				}

				var viableImages = _cachedImages[channelId.Value].Where(img => posts.All(post => post.Key != img.Id));

				if (viableImages.Count() == 0)
				{
					time = DateTime.Now - TimeSpan.FromDays(60);
					posts = (await context.WaifuJoiContentPost.Where(post => post.Time > time).OrderBy(p => p.Time).ToArrayAsync(cancellationToken)).GroupBy(p => p.ContentId).ToDictionary(p => p.Key, p => p.Max(s => s.Time));
					viableImages = _cachedImages[channelId.Value].Where(img => posts.All(post => post.Key != img.Id));
				}

				if (viableImages.Count() == 0)
				{
					time = DateTime.Now - TimeSpan.FromDays(30);
					posts = (await context.WaifuJoiContentPost.Where(post => post.Time > time).OrderBy(p => p.Time).ToArrayAsync(cancellationToken)).GroupBy(p => p.ContentId).ToDictionary(p => p.Key, p => p.Max(s => s.Time));
					viableImages = _cachedImages[channelId.Value].Where(img => posts.All(post => post.Key != img.Id));
				}

				imageToPost = viableImages.Skip(Helpers.RandomGenerator.RandomInt(0, viableImages.Count() - 1)).FirstOrDefault();

				if (imageToPost == null)
				{
					var groupedPosts = posts.GroupBy(p => p.Key).OrderBy(gp => gp.Max(g => g.Value));

					// Get longest non-posted
					foreach (var post in groupedPosts)
					{
						imageToPost = _cachedImages[channelId.Value].FirstOrDefault(cImg => cImg.Id == post.First().Key);
						if (imageToPost != null)
						{
							break;
						}
					}
				}
			}
			else
			{
				var viableImages = _cachedImages.First().Value.Where(img => posts.All(post => post.Key != img.Id));

				imageToPost = viableImages.Skip(Helpers.RandomGenerator.RandomInt(0, viableImages.Count() - 1)).FirstOrDefault();

				if (imageToPost == null)
				{
					var groupedPosts = posts.GroupBy(p => p.Key).OrderBy(gp => gp.Max(g => g.Value));

					// Get longest non-posted
					foreach (var post in groupedPosts)
					{
						imageToPost = _cachedImages.First().Value.FirstOrDefault(cImg => cImg.Id == post.First().Key);
						if (imageToPost != null)
						{
							break;
						}
					}
				}
			}

			return imageToPost;
		}

		public async Task PostRandom(DiscordChannel channel, CancellationToken cancellationToken)
		{
			Console.WriteLine($"Waifubot: PostRandom {channel.Id}");
			using DiscordContext context = new DiscordContext();

			var channelId = Convert.ToInt64(channel.Id);

			var imageToPost = await GetRandomPicture(channelId, cancellationToken);

			if (imageToPost == null)
			{
				return;
			}

			HttpClient client = new HttpClient();
			using var response = await client.GetAsync(GetCreatorUrl(imageToPost.CreatorId), cancellationToken);
			using var stream = await response.Content.ReadAsStreamAsync();
			var creator = await MessagePack.MessagePackSerializer.DeserializeAsync<WaifuJoi.Shared.Features.User.GetUserResponse>(stream);

			DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
			{
				Author = new DiscordEmbedBuilder.EmbedAuthor()
				{
					Name = creator.User.Name,
					IconUrl = BaseAddress + "api/thumbnail/" + creator.User.Avatar,
					Url = BaseAddress + "/profile/" + creator.User.Id
				},
				Color = new DiscordColor("#cf5ed4"),
				Title = context.Puns.Skip(Helpers.RandomGenerator.RandomInt(0, context.Puns.Count() - 1)).First()
					.Text,
				ImageUrl = GetImageUrl(imageToPost.Id)
			};
			Console.WriteLine("WaifuBot: Sending embed");
			_ = channel.SendMessageAsync(embed: builder.Build());
			Console.WriteLine("Waifubot: Sending embed finished");
			context.WaifuJoiContentPost.Add(new WaifuJoiContentPost()
			{
				ContentId = imageToPost.Id,
				Time = DateTime.Now
			});

			await context.SaveChangesAsync();
		}

		public Task Start()
		{
			MainThread = new Thread(async () =>
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
				_updateCacheTimer.Start();

				_announceTimer = new Timer(TimeSpan.FromMinutes(20).TotalMilliseconds)
				{
					AutoReset = true
				};
				_announceTimer.Elapsed += async (object sender, ElapsedEventArgs e) => await AnnounceNewestContent();
				_announceTimer.Start();

				await AnnounceNewestContent();
				await RefreshCache();
			});

			MainThread.Start();

			return Task.CompletedTask;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing)
			{
				_cancellationTokenSource.Cancel();
				_cancellationTokenSource.Dispose();

				_updateCacheTimer.Stop();
				_postTimer.Stop();
				_announceTimer.Stop();
				_httpClient.Dispose();
				_postTimer.Dispose();
				_updateCacheTimer.Dispose();
				_announceTimer.Dispose();
			}

			disposed = true;
		}

		private async Task AnnounceNewestContent()
		{
			WaifuJoi.Shared.Features.Content.SearchContentResponse search = null;

			try
			{
				using var response = await _httpClient.GetAsync(BaseAddress + WaifuJoi.Shared.Features.Content.SearchContentRequest.Route, _cancellationTokenSource.Token);

				if (!response.IsSuccessStatusCode)
				{
					Console.WriteLine("Cannot get newest Content from WaifuJoi");
					return;
				}

				search = await MessagePack.MessagePackSerializer.DeserializeAsync<WaifuJoi.Shared.Features.Content.SearchContentResponse>(await response.Content.ReadAsStreamAsync());
			}
			catch (Exception ex)
			{
				Console.WriteLine("Cannot get newest Content from WaifuJoi");
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}

			using DiscordContext context = new DiscordContext();

			var channelIDs = context.SabrinaSettings.Where(ss => ss.ContentChannel != null).Select(ss => ss.ContentChannel.Value).ToArray();
			List<DiscordChannel> channelsToPostTo = new List<DiscordChannel>();

			foreach (var channelId in channelIDs)
			{
				if (!_client.Guilds.Any())
				{
					try
					{
						channelsToPostTo.Add(await _client.GetChannelAsync(Convert.ToUInt64(channelId)));
					}
					catch (Exception) { }
				}
				else if (_client.Guilds.Any(g => g.Value.Channels.Any(c => c.Key == Convert.ToUInt64(channelId))))
				{
					channelsToPostTo.Add(await _client.GetChannelAsync(Convert.ToUInt64(channelId)));
				}
			}

			foreach (var channel in channelsToPostTo)
			{
				var channelIdLong = Convert.ToInt64(channel.Id);

				var settings = context.SabrinaSettings.First(ss => ss.ContentChannel == channelIdLong);

				var newContent = search.Content.Where(c => settings.LastWaifuJoiUpdate == null || c.CreationDate > settings.LastWaifuJoiUpdate);

				foreach (var content in newContent)
				{
					DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
					{
						Color = new DiscordColor(184, 86, 185),
						ImageUrl = BaseAddress + "api/thumbnail/" + content.Id.Trim(),
						Title = content.Title,
						Description = content.Description,
						Url = BaseAddress + "show/" + content.Id.Trim(),
						Timestamp = content.CreationDate,
						Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = BaseAddress + "favicon.ico", Text = $"Hosted with <3 by Waifujoi" }
					};

					if (content.Creator.DiscordId != null)
					{
						try
						{
							var creator = await _client.GetUserAsync(Convert.ToUInt64(content.Creator.DiscordId.Value));
							builder.AddField("Creator", creator.Mention);
						}
						catch (Exception)
						{ }
					}
					else
					{
						builder.AddField("Creator", content.Creator.Name);
					}

					await channel.SendMessageAsync(embed: builder).ConfigureAwait(false);
				}

				settings.LastWaifuJoiUpdate = DateTime.Now;
			}

			await context.SaveChangesAsync(_cancellationTokenSource.Token);
		}

		private async Task PostToAll()
		{
			Console.WriteLine("WaifuBot: Sending to all");
			using DiscordContext context = new DiscordContext();

			var channels = context.SabrinaSettings.Where(ss => ss.FeetChannel != null).Select(setting => setting.FeetChannel.Value);

			foreach (var channelId in channels)
			{
				DiscordChannel channel = null;

				if (!_cancellationTokenSource.Token.IsCancellationRequested && _client.Guilds.Any(g => g.Value.Channels.Any(c => c.Key == Convert.ToUInt64(channelId))))
				{
					channel = await _client.GetChannelAsync(Convert.ToUInt64(channelId));

					try
					{
						await PostRandom(channel, _cancellationTokenSource.Token);
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
			Console.WriteLine("WaifuBot: refreshing cache");
			_cachedImages.Clear();

			using DiscordContext context = new DiscordContext();

			var groupedChannels = context.WaifuJoiAlbum.ToLookup(album => album.ChannelId);
			Random rnd = new Random();

			foreach (var group in groupedChannels)
			{
				List<Content> pictures = new List<Content>();

				foreach (var album in group)
				{
					using var response = await _httpClient.GetAsync(
						BaseAddress + WaifuJoi.Shared.Features.Content.SearchContentRequest.Route + $"?albumid={album.ContentId}&contenttypes={(int)Content.ContentType.AlbumPicture}",
						_cancellationTokenSource.Token);

					if (!response.IsSuccessStatusCode)
					{
						continue;
					}

					using var stream = await response.Content.ReadAsStreamAsync();
					var model = await MessagePack.MessagePackSerializer.DeserializeAsync<WaifuJoi.Shared.Features.Content.GetAlbumResponse>(stream);

					pictures.AddRange(model.Content);
				}

				_cachedImages.Add(group.Key, pictures);
			}

			Console.WriteLine("WaifuBot: Done refreshing cache");
		}
	}
}