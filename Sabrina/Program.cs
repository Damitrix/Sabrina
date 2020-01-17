// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="SalemsTools">
//     Do whatever
// </copyright>
// <summary>
// Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Configuration;

namespace Sabrina
{
	using DSharpPlus;
	using DSharpPlus.CommandsNext;
	using DSharpPlus.Entities;
	using DSharpPlus.EventArgs;
	using DSharpPlus.Interactivity;
	using Microsoft.EntityFrameworkCore;
	using Microsoft.Extensions.DependencyInjection;
	using Models;
	using Sabrina.Bots;
	using Sabrina.Pornhub;
	using System;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;

	internal class Program
	{
		private const string Prefix = "//";

		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private DiscordClient _client;
		private DiscordContext _context;
		private EventBot _eventBot;
#pragma warning disable IDE0052 // Remove unread private members
		private bool _guildsAvailable = false;
		private HelpBot _helpBot;
		private DateTime _lastCheeseHorny = DateTime.MinValue;
		private DateTime _lastHiMark = DateTime.MinValue;
		private PornhubBot _pornhubBot;
#pragma warning restore IDE0052 // Remove unread private members
		private SankakuBot _sankakuBot;

		//private TumblrBot _tmblrBot;
		private WaifuJOIBot _waifujoiBot;

		public CommandsNextExtension Commands { get; set; }
		public InteractivityExtension Interactivity { get; set; }

		/// <summary>
		/// The main.
		/// </summary>
		/// <param name="args">The args.</param>
		public static Task Main(string[] args)
		{
			AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
			{
				Console.WriteLine(eventArgs.Exception.ToString());
			};

			var prog = new Program();
			try
			{
				return prog.MainAsync(args);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.InnerException);
				Console.ReadKey();
				//throw ex;
			}

			return Task.CompletedTask;
		}

		public void Dispose()
		{
			this._client.Dispose();
		}

		public async Task MainAsync(string[] args)
		{
			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

			this.SetConfig(args);
			this.CreateFolders();

			_client.GuildDownloadCompleted += _client_GuildDownloadCompleted;

			await this._client.ConnectAsync();
			this._client.MessageCreated += this.ClientMessageCreated;
			this._client.GuildMemberUpdated += this.ClientGuildMemberUpdated;

			await this._client.UpdateStatusAsync(new DiscordActivity("you", ActivityType.Watching), UserStatus.Online);

			await AnnounceVersion();

			_client.ClientErrored += Client_ClientErrored;

			await Task.Run(async () => await StartBots());

			await Task.Run(() =>
			{
				var exit = false;
				while (!exit)
				{
					string command = Console.ReadLine();
					switch (command)
					{
						case "stop":
						case "exit":
						case "x":
							exit = true;
							Exit();
							break;
					}
				}
			});

			await this._client.DisconnectAsync();
		}

		private Task _client_GuildDownloadCompleted(GuildDownloadCompletedEventArgs e)
		{
			_guildsAvailable = true;
			return Task.CompletedTask;
		}

		private async Task AnnounceVersion()
		{
			using var context = new DiscordContext();
			var latestVersion = await context.SabrinaVersion.OrderByDescending(v => v.VersionNumber).FirstOrDefaultAsync();

			if (latestVersion == null || latestVersion.WasAnnounced == 1)
			{
				return;
			}

			foreach (var setting in context.SabrinaSettings)
			{
				long channelId = 0;

				if (setting.WheelChannel != null)
				{
					channelId = setting.WheelChannel.Value;
				}
				else if (setting.ContentChannel != null)
				{
					channelId = setting.ContentChannel.Value;
				}
				else if (setting.FeetChannel != null)
				{
					channelId = setting.FeetChannel.Value;
				}

				if (channelId == 0)
				{
					continue;
				}

				try
				{
					var channel = await _client.GetChannelAsync(Convert.ToUInt64(channelId));

					DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
					{
						Title = $"I've received an upgrade! (v{latestVersion.VersionNumber})",
						Description = latestVersion.Description.Replace("\\n", Environment.NewLine),
						Color = new DiscordColor(70, 254, 31)
					};

					await channel.SendMessageAsync(embed: builder.Build());
				}
				catch (Exception)
				{
				}
			}

			latestVersion.WasAnnounced = 1;

			await context.SaveChangesAsync();
		}

		private Task Client_ClientErrored(ClientErrorEventArgs e)
		{
			Console.WriteLine("Client Error occured:");
			Console.WriteLine("Event: " + e.EventName);
			Console.WriteLine("Message: " + e.Exception.Message);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Call when a Discord Client gets updated. Used to combat nickname-rename shenanigans by Obe
		/// </summary>
		/// <param name="e">The Event Args.</param>
		/// <returns>The <see cref="Task"/>.</returns>
		private async Task ClientGuildMemberUpdated(GuildMemberUpdateEventArgs e)
		{
			if (e.Guild.Permissions == null || !e.Guild.Permissions.Value.HasFlag(Permissions.ManageNicknames))
			{
				return;
			}

			if (e.Member.Id == 450771319479599114)
			{
				await (await e.Guild.GetMemberAsync(450771319479599114)).ModifyAsync(e => e.Nickname = "Sabrina");
			}

			if (e.Member.Id == 249216025931939841)
			{
				await (await e.Guild.GetMemberAsync(249216025931939841)).ModifyAsync(e => e.Nickname = "Salem");
			}
		}

		/// <summary>
		/// Call when client Message is created. Logs all Messages to Database.
		/// </summary>
		/// <param name="e">The Message Event Args.</param>
		/// <returns>A <see cref="Task"/>.</returns>
		private async Task ClientMessageCreated(MessageCreateEventArgs e)
		{
			var ohMatch = Regex.Match(e.Message.Content, "oh([.,!,?]?)", RegexOptions.IgnoreCase);

			if (ohMatch.Success && e.Message.Content.Length < 4 && _lastHiMark < DateTime.Now - TimeSpan.FromMinutes(60))
			{
				string himark = e.Message.Content.StartsWith("OH") ? "HI MARK" : "Hi Mark";
				string mark = ohMatch.Groups.Count > 0 && ohMatch.Groups[1].Value.Contains('.', '?', '!') ? ohMatch.Groups[1].Value : "";
				await e.Channel.SendMessageAsync($"{himark}{mark}");
				_lastHiMark = DateTime.Now;
			}

			if (e.Author.Id == 503662068050952192 && e.Message.Content.Contains("horny", StringComparison.InvariantCultureIgnoreCase) && _lastCheeseHorny < DateTime.Now - TimeSpan.FromMinutes(60))
			{
				await e.Channel.SendMessageAsync("Yes Cheese, we know you're horny");
				_lastCheeseHorny = DateTime.Now;
			}

			using var context = new DiscordContext();

			if (context.Messages.Any(msg => msg.MessageId == Convert.ToInt64(e.Message.Id)))
			{
				return;
			}

			var user = await UserExtension.GetUser(e.Message.Author.Id, context);

			var cMsg = new Messages()
			{
				AuthorId = Convert.ToInt64(user.UserId),
				MessageText = e.Message.Content,
				ChannelId = Convert.ToInt64(e.Message.Channel.Id),
				CreationDate = e.Message.CreationTimestamp.DateTime,
				MessageId = Convert.ToInt64(e.Message.Id)
			};

			try
			{
				user = await UserExtension.GetUser(e.Message.Author.Id, context);

				await _context.Messages.AddAsync(cMsg);
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error trying to enter Message into Database");
				Console.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// Create missing folders.
		/// </summary>
		private void CreateFolders()
		{
			if (!Directory.Exists(Config.BotFileFolders.SlaveReports))
			{
				Directory.CreateDirectory(Config.BotFileFolders.SlaveReports);
			}

			if (!Directory.Exists(Config.BotFileFolders.WheelResponses))
			{
				Directory.CreateDirectory(Config.BotFileFolders.WheelResponses);
			}

			if (!Directory.Exists(Config.BotFileFolders.WheelLinks))
			{
				Directory.CreateDirectory(Config.BotFileFolders.WheelLinks);
			}

			if (!Directory.Exists(Config.BotFileFolders.UserData))
			{
				Directory.CreateDirectory(Config.BotFileFolders.UserData);
			}

			if (!Directory.Exists(Config.BotFileFolders.Media))
			{
				Directory.CreateDirectory(Config.BotFileFolders.Media);
			}
		}

		private void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			Exit();
		}

		private void Exit()
		{
			try
			{
				_cancellationTokenSource.Cancel();
				_cancellationTokenSource.Dispose();
				_pornhubBot?.Dispose();
				_sankakuBot?.Dispose();
				_waifujoiBot?.Dispose();
				_helpBot?.Dispose();
				_eventBot?.Dispose();
			}
			catch (TaskCanceledException)
			{
			}

			Console.WriteLine("Exiting");
		}

		private void SetCommands()
		{
			var serviceCollection = new ServiceCollection();

			serviceCollection.AddSingleton(this._sankakuBot);
			serviceCollection.AddSingleton(this._waifujoiBot);

			var ccfg = new CommandsNextConfiguration()
			{
				CaseSensitive = false,
				EnableDefaultHelp = true,
				EnableDms = false,
				EnableMentionPrefix = true,
				IgnoreExtraArguments = true,
				StringPrefixes = new[] { Prefix },
				Services = serviceCollection.BuildServiceProvider()
			};

			this._context = new DiscordContext();
			this.Commands = this._client.UseCommandsNext(ccfg);

			this.Commands.RegisterCommands(Assembly.GetExecutingAssembly());
		}

		private void SetConfig(string[] args)
		{
			if (args.Length > 0)
			{
				Config.ConfigPath = args[0];
			}

			var config = new DiscordConfiguration
			{
				AutoReconnect = true,
				MessageCacheSize = 2048,
				LogLevel = LogLevel.Debug,
				Token = Config.Token,
				TokenType = TokenType.Bot,
				UseInternalLogHandler = true
			};

			this._client = new DiscordClient(config);

			this.Interactivity = this._client.UseInteractivity(
				new InteractivityConfiguration()
				{
					PaginationBehaviour = DSharpPlus.Interactivity.Enums.PaginationBehaviour.Default,
					Timeout = TimeSpan.FromSeconds(30)
				});
		}

		private async Task StartBots()
		{
			while (!_guildsAvailable)
			{
				await Task.Delay(100);
			}

			Console.WriteLine("Guilds available. Starting bots");

			try
			{
				_sankakuBot = new SankakuBot(this._client); // Is Seperate Thread
				_sankakuBot.Initialize();

				_waifujoiBot = new WaifuJOIBot(this._client); // Is Seperate Thread
				await _waifujoiBot.Start();

				_eventBot = new EventBot(this._client, _cancellationTokenSource.Token); // Is Seperate Thread
				await _eventBot.Start();

				_pornhubBot = new PornhubBot(this._client, _cancellationTokenSource.Token); // Is Seperate Thread

				_helpBot = new HelpBot(this._client); // Is Seperate Thread

				this.SetCommands();
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);
			}
		}
	}
}