using Configuration;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Sabrina.Entities;
using Sabrina.Models;
using Sabrina.SankakuModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
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
	public class SankakuBot : IDisposable
	{
		private const string _baseUrl = "https://" + _domain;
		private const string _domain = "capi-v2.sankakucomplex.com";
		private const string _indexUrl = "/posts";

		private const int _minScore = 300;
		private const int _minWhiteWeight = 29;

		//private const string _tagUrl = "/tags";
		private readonly Dictionary<long, List<long>> _cachedImages = new Dictionary<long, List<long>>();

		private readonly DiscordClient _client;
		private readonly HttpClient _httpClient;
		private readonly ConcurrentBag<long> _postedImages = new ConcurrentBag<long>();
		private Thread _mainThread;
		private Timer _postTimer;
		private Timer _scrapeNewestTimer;
		private Timer _scrapeRandomTimer;
		private Timer _updateCacheTimer;

		private bool disposed = false;

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
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public async Task<(Entities.Sankaku.Image Information, byte[] Data)> GetImage(DiscordChannel channel, int skip = 0)
		{
			using var sankakuContext = new SankakuContext();

			var channelIDLong = Convert.ToInt64(channel.Id);

			var startDate = DateTime.Now - TimeSpan.FromDays(90);

			if (!_cachedImages.ContainsKey(channelIDLong))
			{
				Console.WriteLine($"Couldn't find cached Images for Channel {channelIDLong}");
				throw new Exception("Couldn't find cached Images for Channel.");
			}

			var images = _cachedImages[Convert.ToInt64(channelIDLong)];

			async Task RemoveImage(long image)
			{
				images.Remove(image);
				var sankakuImage = await sankakuContext.Image.FirstOrDefaultAsync();
				if (sankakuImage != null)
				{
					sankakuImage.IsAvailableBool = false;
					await sankakuContext.SaveChangesAsync();
				}
			}

			var hasImage = images.Where(cImg => !_postedImages.Contains(cImg)).Skip(skip).Any();

			if (hasImage == false)
			{
				await Console.Error.WriteLineAsync("Could not find a suitable Sankaku Image");
				throw new Exception("Could not find a suitable Sankaku Image.");
			}

			Uri imageUri = null;
			int attempt = 0;

			Entities.Sankaku.Image imgToPost = null;

			while (imageUri == null && attempt < 10)
			{
				var imgToPostID = images.Where(cImg => !_postedImages.Contains(cImg)).Skip(skip).FirstOrDefault();

				if (imgToPostID == default)
				{
					await Console.Error.WriteLineAsync("Could not find a suitable Sankaku Image in loop.");
					throw new Exception("Could not find a suitable Sankaku Image in loop.");
				}

				imgToPost = await GetOriginalImage(imgToPostID);

				if (imgToPost == null || imgToPost.Status != Entities.Sankaku.Status.Active || imgToPost.FileUrl == null || imgToPost.FileSize > 8_000_000)
				{
					if (imgToPost == null)
					{
						await Console.Error.WriteLineAsync($"Could not retrieve image with nr. {imgToPost.Id}");
					}
					else if (imgToPost.Status != Entities.Sankaku.Status.Active)
					{
						await Console.Error.WriteLineAsync($"Cannot send image with nr. {imgToPost.Id} because its Status is not \"Active\"");
					}
					else if (imgToPost.FileUrl == null)
					{
						await Console.Error.WriteLineAsync($"Cannot send image with nr. {imgToPost.Id} because its FileUrl is null");
					}
					else if (imgToPost.FileSize > 8_000_000)
					{
						await Console.Error.WriteLineAsync($"Cannot send image with nr. {imgToPost.Id} because it's too big");
					}

					await RemoveImage(imgToPost.Id);
					await Task.Delay(100);
				}
				else
				{
					imageUri = imgToPost.FileUrl;
					break;
				}

				attempt++;
			}

			if (imageUri == null)
			{
				await Console.Error.WriteLineAsync($"Attempt {attempt} of retrieving sankaku Image has not succeeded.");
				throw new Exception($"Attempt {attempt} of retrieving sankaku Image has not succeeded.");
			}

			var link = HttpUtility.HtmlDecode(imageUri.AbsoluteUri);

			HttpResponseMessage response = null;

			try
			{
				response = await _httpClient.GetAsync(link);
			}
			catch (Exception)
			{ }

			if (response.StatusCode != HttpStatusCode.OK)
			{
				await Console.Error.WriteLineAsync($"Could not get Image Link for {imgToPost}").ConfigureAwait(false);
				await RemoveImage(imgToPost.Id);
				throw new Exception($"Could not get Image Link.");
			}

			return (imgToPost, await response.Content.ReadAsByteArrayAsync());
		}

		public void Initialize()
		{
			_mainThread = new Thread(() => MainTask().GetAwaiter().GetResult())
			{
				Name = "SankakuThread"
			};
			_mainThread.Start();
		}

		public async Task<bool> PostRandom(DiscordChannel channel, int skip = 0)
		{
			using var discordContext = new DiscordContext();

			(Entities.Sankaku.Image Information, byte[] Data)? image = null;

			try
			{
				image = await GetImage(channel, skip);
			}
			catch (Exception)
			{
				return false;
			}

			string extension = ".jpeg";

			switch (image.Value.Information.FileType)
			{
				case Entities.Sankaku.FileType.ImageGif:
					extension = ".gif";
					break;

				case Entities.Sankaku.FileType.ImageJpeg:
					extension = ".jpeg";
					break;

				case Entities.Sankaku.FileType.ImagePng:
					extension = ".png";
					break;
			}

			DiscordMessage msg = null;

			var outStream = new MemoryStream(image.Value.Data);

			DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
			{
				Color = DiscordColor.SpringGreen,
				Title = discordContext.Puns.Skip(Helpers.RandomGenerator.RandomInt(0, discordContext.Puns.Count() - 1)).First().Text
			};

			try
			{
				if (outStream == null)
				{
					Console.WriteLine("SankakuBot: Outstream was null");
				}

				Console.WriteLine($"SankakuBot: Sending File {Helpers.GetSafeFilename(builder.Title + extension)} to channel \"{channel.Name}\"");
				var msgTask = channel.SendFileAsync(Helpers.GetSafeFilename(builder.Title + extension), outStream, embed: builder.Build());

				int index = 0;
				while (!msgTask.IsCompleted && !msgTask.IsFaulted && index < 6000)
				{
					await Task.Delay(5);
				}

				if (msgTask.IsCompleted)
				{
					msg = await msgTask;
				}
				else
				{
					Console.WriteLine($"SankakuBot: Couldn't send File with name {Helpers.GetSafeFilename(builder.Title + extension)}");
					outStream.Close();
					await _client.ReconnectAsync(true);
				}

				Console.WriteLine("SankakuBot: Finished Sending File");
			}
			catch (Exception ex)
			{
				Console.WriteLine("SankakuBot: Something went wrong while trying to send file");
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.InnerException);
			}
			finally
			{
				try
				{
					var taskContext = new DiscordContext();

					taskContext.SankakuPost.Add(new SankakuPost()
					{
						Date = DateTime.Now,
						ImageId = image.Value.Information.Id,
						MessageId = Convert.ToInt64(msg.Id)
					});

					await taskContext.SaveChangesAsync();

					_postedImages.Add(image.Value.Information.Id);

					outStream.Close();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}

			return true;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing)
			{
				try
				{
					_scrapeNewestTimer.Stop();
					_scrapeRandomTimer.Stop();
					_updateCacheTimer.Stop();

					_httpClient.Dispose();
					_postTimer.Dispose();
					_scrapeNewestTimer.Dispose();
					_scrapeRandomTimer.Dispose();
					_updateCacheTimer.Dispose();
					_mainThread.Abort();
				}
				catch (PlatformNotSupportedException)
				{ }
				catch (TaskCanceledException)
				{ }
			}

			disposed = true;
		}

		private async Task Client_MessageReactionAdded(DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
		{
			using var context = new DiscordContext();
			using var sankakuContext = new SankakuModel.SankakuContext();

			var message = await context.SankakuPost.FirstOrDefaultAsync(post => post.MessageId == Convert.ToInt64(e.Message.Id));

			if (message != null)
			{
				SankakuModel.ImageVote vote = null;

				vote = new SankakuModel.ImageVote()
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
				else if (discordName == Config.Emojis.Underage)
				{
					await e.Message.DeleteAsync("Underage");
					await e.Message.RespondAsync("Oopsie");
				}

				if (vote.VoteValue != 0)
				{
					await sankakuContext.ImageVote.AddAsync(vote);
				}

				await sankakuContext.SaveChangesAsync();
			}
		}

		private async Task Client_MessageReactionRemoved(DSharpPlus.EventArgs.MessageReactionRemoveEventArgs e)
		{
			using var sankakuContext = new SankakuModel.SankakuContext();
			using var discordContext = new DiscordContext();

			var message = await discordContext.SankakuPost.FirstOrDefaultAsync(post => post.MessageId == Convert.ToInt64(e.Message.Id));

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

				var vote = await sankakuContext.ImageVote.FirstOrDefaultAsync(sankakuVote => sankakuVote.ImageId == message.ImageId &&
																							 sankakuVote.UserId == Convert.ToInt64(e.User.Id) &&
																							 sankakuVote.VoteValue == VoteValue);

				if (vote != null)
				{
					sankakuContext.ImageVote.Remove(vote);
				}

				await sankakuContext.SaveChangesAsync();
			}
		}

		private async Task<Entities.Sankaku.Image> GetOriginalImage(long id)
		{
			var response = await _httpClient.GetAsync(_baseUrl + _indexUrl + $"?limit=1&page=1&tags=id_range:{id}");

			if (response.StatusCode != HttpStatusCode.OK)
			{
				return null;
			}

			var json = await response.Content.ReadAsStringAsync();

			var image = Entities.Sankaku.Image.FromJson(json).FirstOrDefault();

			return image;
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

			_scrapeRandomTimer = new Timer(TimeSpan.FromMinutes(2).TotalMilliseconds)
			{
				AutoReset = true
			};

			_scrapeRandomTimer.Elapsed += async (object o, ElapsedEventArgs e) => await Task.Run(async () =>
			{
				try
				{
					await Scrape(Order.Random);
				}
				catch (Exception ex)
				{
					Console.WriteLine("SankakuBot: Error while scraping");
					Console.WriteLine(ex.Message);
					Console.WriteLine(ex.StackTrace);
				}
			});

			_scrapeRandomTimer.Start();

			_scrapeNewestTimer = new Timer(TimeSpan.FromHours(1).TotalMilliseconds)
			{
				AutoReset = true
			};

			_scrapeNewestTimer.Elapsed += async (object o, ElapsedEventArgs e) => await Task.Run(async () => await Scrape(Order.Newest));

			_scrapeNewestTimer.Start();

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

			using DiscordContext context = new DiscordContext();

			if (!context.SankakuPost.Any() || context.SankakuPost.OrderByDescending(sp => sp.Date).First().Date < DateTime.Now - TimeSpan.FromMinutes(20))
			{
				await PostTimer_Elapsed();
			}
		}

		private async Task PostTimer_Elapsed()
		{
			using DiscordContext context = new DiscordContext();

			foreach (var channelId in context.SabrinaSettings.Select(ss => ss.FeetChannel).Where(cId => cId != null))
			{
				DiscordChannel channel = null;

				if (_client.Guilds.Any(g => g.Value.Channels.Any(c => c.Key == Convert.ToUInt64(channelId))))
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
			Console.WriteLine($"SankakuBot: scraping for {order.ToFormattedText()}");
			using var sankakuContext = new SankakuModel.SankakuContext(120, 200);

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

			var response = await _httpClient.GetAsync(_baseUrl + _indexUrl + $"?limit={limit}&tags=order:{orderString}").ConfigureAwait(false);

			if (response.StatusCode != HttpStatusCode.OK)
			{
				return false;
			}

			var json = await response.Content.ReadAsStringAsync();

			var images = Entities.Sankaku.Image.FromJson(json);

			var allTags = sankakuContext.Tag.Select(t => t.Id).ToList();

			foreach (Entities.Sankaku.Image image in images)
			{
				Image dbImage = await sankakuContext.Image.FindAsync(image.Id);

				if (dbImage == null)
				{
					dbImage = new Image()
					{
						Id = image.Id,
						Rating = (int)image.Rating,
						RatingCount = Convert.ToInt32(image.VoteCount),
						Score = Convert.ToInt32(image.TotalScore)
					};

					sankakuContext.Image.Add(dbImage);
				}
				else
				{
					dbImage.Rating = (int)image.Rating;
					dbImage.RatingCount = Convert.ToInt32(image.VoteCount);
					dbImage.Score = Convert.ToInt32(image.TotalScore);
				}

				foreach (var tag in image.Tags)
				{
					if (string.IsNullOrEmpty(tag.Name))
					{
						tag.Name = string.Empty;
					}

					if (allTags.All(savedTag => savedTag != Convert.ToInt32(tag.Id)))
					{
						var dbTag = new Tag()
						{
							Name = tag.Name,
							Id = Convert.ToInt32(tag.Id)
						};

						sankakuContext.Tag.Add(dbTag);
						allTags.Add(dbTag.Id);
					}

					if (dbImage.ImageTag.All(t => t.TagId != tag.Id))
					{
						ImageTag imageTag = new ImageTag()
						{
							TagId = Convert.ToInt32(tag.Id)
						};

						dbImage.ImageTag.Add(imageTag);
					}
				}

				dbImage = null;
			}

			allTags = null;

			await sankakuContext.SaveChangesAsync();

			sankakuContext.Dispose();

			GC.Collect(2);

			return true;
		}

		private async Task UpdateCache()
		{
			Stopwatch sw = new Stopwatch();
			Console.WriteLine("SankakuBot: Updating Cache");

			using var sankakuContext = new SankakuContext(30, 1);
			using var discordContext = new DiscordContext();

			sw.Start();

			var minTime = DateTime.Now - TimeSpan.FromDays(90);

			foreach (var setting in discordContext.SabrinaSettings.Where(ss => ss.FeetChannel != null).ToArray())
			{
				var channel = setting.FeetChannel.Value;

				var channelParam = new SqlParameter("@channel", System.Data.SqlDbType.BigInt)
				{
					Value = channel
				};
				var dateParam = new SqlParameter("@minDate", minTime);
				var minScoreParam = new SqlParameter("@minScore", _minScore);
				var minWhiteWeightParam = new SqlParameter("@minWhiteWeight", _minWhiteWeight);

				using SqlConnection conn = new SqlConnection(Config.AdminConnectionString);

				var cmd = conn.CreateCommand();

				cmd.CommandTimeout = 360;

				cmd.CommandText = @"SELECT TOP(100) [id],
									[score],
									[rating],
									[ratingcount],
									[isavailable]
					FROM   sankaku.dbo.[image] mainImg
					WHERE  (SELECT Sum(weight)
							FROM   sankaku.dbo.[imagetag]
								   JOIN discord.dbo.[sankaku_tagwhitelist]
									 ON discord.dbo.[sankaku_tagwhitelist].tagid =
										sankaku.dbo.[imagetag].tagid
							WHERE  sankaku.dbo.[imagetag].imageid = mainImg.id
								   AND discord.dbo.[sankaku_tagwhitelist].channelid =
									   @channel) >
								  @minWhiteWeight
						   AND (SELECT Count(*)
								FROM   sankaku.dbo.[imagetag]
									   JOIN discord.dbo.[sankaku_tagblacklist]
										 ON discord.dbo.[sankaku_tagblacklist].tagid =
											sankaku.dbo.[imagetag].tagid
								WHERE  sankaku.dbo.[imagetag].imageid = mainImg.id
									   AND discord.dbo.[sankaku_tagblacklist].channelid =
										   @channel) = 0
						   AND ( (SELECT Count(1) Date
								  FROM   discord.dbo.[sankakupost]
								  WHERE  discord.dbo.[sankakupost].imageid = mainImg.id) = 0
								  OR (SELECT TOP(1) date
									  FROM   discord.dbo.[sankakupost]
									  WHERE  discord.dbo.[sankakupost].imageid = mainImg.id
									  ORDER  BY date DESC) < @minDate )
						   AND mainImg.score > @minScore
						   AND mainImg.isavailable = 1";

				cmd.Parameters.Add(channelParam);
				cmd.Parameters.Add(dateParam);
				cmd.Parameters.Add(minScoreParam);
				cmd.Parameters.Add(minWhiteWeightParam);

				await conn.OpenAsync();
				var reader = await cmd.ExecuteReaderAsync();

				List<Image> cachedImages = new List<Image>();

				while (await reader.ReadAsync())
				{
					cachedImages.Add(new Image()
					{
						Id = reader.GetInt64(0),
						Score = reader.GetInt32(1),
						Rating = reader.GetInt32(2),
						RatingCount = reader.GetInt32(3),
						IsAvailable = reader.GetInt16(4)
					});
				}

				await conn.CloseAsync();

				if (!_cachedImages.ContainsKey(channel))
				{
					_cachedImages.Add(channel, new List<long>());
				}
				else
				{
					_cachedImages[channel].Clear();
				}

				_cachedImages[channel] = cachedImages.Select(img => img.Id).ToList();
			}

			_postedImages.Clear();

			sankakuContext.Dispose();
			Console.WriteLine($"Finished updating Sankaku Cache after {sw.Elapsed}");
		}
	}
}