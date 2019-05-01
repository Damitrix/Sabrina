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
    using Models;
    using Sabrina.Bots;
    using Sabrina.Pornhub;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    internal class Program
    {
        private const string Prefix = "//";

        private DiscordClient _client;
        private DiscordContext _context;
        private SankakuBot _sankakuBot;

        //private TumblrBot _tmblrBot;
        private WaifuJOIBot _waifujoiBot;

        public CommandsNextModule Commands { get; set; }
        public InteractivityModule Interactivity { get; set; }
        public object Voice { get; private set; }

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">The args.</param>
        public static void Main(string[] args)
        {
            var prog = new Program();
            try
            {
                prog.MainAsync(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                Console.ReadKey();
                //throw ex;
            }
        }

        public void Dispose()
        {
            this._client.Dispose();
        }

        public async Task MainAsync(string[] args)
        {
            this.SetConfig(args);

            _sankakuBot = new SankakuBot(this._client); // Is Seperate Thread
            _sankakuBot.Initialize();

            _waifujoiBot = new WaifuJOIBot(this._client); // Is Seperate Thread
            await _waifujoiBot.Start();

            this.SetCommands();
            this.CreateFolders();

            await this._client.ConnectAsync();
            this._client.MessageCreated += this.ClientMessageCreated;
            this._client.GuildMemberUpdated += this.ClientGuildMemberUpdated;

            await this._client.UpdateStatusAsync(new DiscordGame("Feetsies"), UserStatus.Online);

            // TODO: Looks weird, cause unused.
            try
            {
                PornhubBot pornhubBot = new PornhubBot(this._client); // Is Seperate Thread

                HelpBot helpBot = new HelpBot(this._client); // Is Seperate Thread
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }

            await AnnounceVersion();

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
                            PornhubBot.Exit = true;
                            exit = true;
                            break;
                    }
                }
            });

            await this._client.DisconnectAsync();
        }

        private async Task AnnounceVersion()
        {
            var context = new DiscordContext();

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

        /// <summary>
        /// Call when a Discord Client gets updated. Used to combat nickname-rename shenanigans by Obe
        /// </summary>
        /// <param name="e">The Event Args.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        private async Task ClientGuildMemberUpdated(GuildMemberUpdateEventArgs e)
        {
            if (e.Member.Id == 450771319479599114)
            {
                await (await e.Guild.GetMemberAsync(450771319479599114)).ModifyAsync(
                    nickname: "Sabrina");
            }

            if (e.Member.Id == 249216025931939841)
            {
                await (await e.Guild.GetMemberAsync(249216025931939841)).ModifyAsync(
                    nickname: "Salem");
            }
        }

        /// <summary>
        /// Call when client Message is created. Logs all Messages to Database.
        /// </summary>
        /// <param name="e">The Message Event Args.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        private async Task ClientMessageCreated(MessageCreateEventArgs e)
        {
            var context = new DiscordContext();

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

        private void SetCommands()
        {
            DependencyCollection dep = null;
            using (var d = new DependencyCollectionBuilder())
            {
                d.AddInstance(new Dependencies()
                {
                    SankakuBot = this._sankakuBot,
                    WaifuJoiBot = this._waifujoiBot
                });
                dep = d.Build();
            }

            var ccfg = new CommandsNextConfiguration()
            {
                CaseSensitive = false,
                EnableDefaultHelp = true,
                EnableDms = false,
                EnableMentionPrefix = true,
                IgnoreExtraArguments = true,
                StringPrefix = Prefix,
                Dependencies = dep
            };

            this._context = new DiscordContext();
            this.Commands = this._client.UseCommandsNext(ccfg);

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && t.Namespace == "Sabrina.Commands" && t.DeclaringType == null))
            {
                if (type == null || type.Name == "BlackJackGame" || type.IsAbstract || type.FullName == "Sabrina.Commands.Edges+<AssignEdgesAsync>d__2") //Really shitty solution, but im lazy
                {
                    continue;
                }
                var info = type.GetTypeInfo();
                this.Commands.RegisterCommands(type);
            }
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
                    PaginationBehaviour = TimeoutBehaviour.Default,
                    PaginationTimeout = TimeSpan.FromSeconds(30),
                    Timeout = TimeSpan.FromSeconds(30)
                });
        }
    }
}