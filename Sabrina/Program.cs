// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="SalemsTools">
//   Do whatever
// </copyright>
// <summary>
//   Defines the Program type.
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
    using Models;
    using Sabrina.Bots;
    using Sabrina.Pornhub;
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    internal class Program
    {
        private const string Prefix = "//";

        private DiscordContext _context;
        private DiscordClient client;

        private SqlConnection conn;
        private TumblrBot tmblrBot;
        public CommandsNextModule Commands { get; set; }

        public InteractivityModule Interactivity { get; set; }
        public object Voice { get; private set; }

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
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
                Console.ReadKey();
                //throw ex;
            }
        }

        public void Dispose()
        {
            this.client.Dispose();
        }

        public async Task MainAsync(string[] args)
        {
            this.SetConfig(args);
            this.SetCommands();
            this.CreateFolders();

            await this.client.ConnectAsync();
            this.client.Ready += this.OnReadyAsync;
            this.client.MessageCreated += this.ClientMessageCreated;
            this.client.MessageReactionAdded += this.ClientMessageReactionAdded;
            this.client.GuildMemberUpdated += this.ClientGuildMemberUpdated;

            this.conn = new SqlConnection(Config.DatabaseConnectionString);
            await this.conn.OpenAsync();

            await this.client.UpdateStatusAsync(new DiscordGame("Feetsies"), UserStatus.Online);

            // TODO: Looks weird, cause unused.
            try
            {
                SankakuBot sankakuBot = new SankakuBot(this.client);
                sankakuBot.Initialize();

                PornhubBot pornhubBot = new PornhubBot(this.client);

                HelpBot helpBot = new HelpBot(this.client);

                this.tmblrBot = new TumblrBot(this.client);
                await tmblrBot.InitializeAsync();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
            

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

            await this.client.DisconnectAsync();
        }

        /// <summary>
        /// Call when a Discord Client gets updated. Used to combat nickname-rename shenanigans by Obe
        /// </summary>
        /// <param name="e">
        /// The Event Args.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here. Obe is a Name")]
        private async Task ClientGuildMemberUpdated(GuildMemberUpdateEventArgs e)
        {
            if (e.Member.Id == 450771319479599114)
            {
                await (await e.Guild.GetMemberAsync(450771319479599114)).ModifyAsync(
                    nickname: "Sabrina");
            }
        }

        /// <summary>
        /// Call when client Message is created. Logs all Messages to Database.
        /// </summary>
        /// <param name="e">
        /// The Message Event Args.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/>.
        /// </returns>
        private async Task ClientMessageCreated(MessageCreateEventArgs e)
        {
            var msg = new Messages()
            {
                AuthorId = Convert.ToInt64(e.Author.Id),
                MessageText = e.Message.Content,
                ChannelId = Convert.ToInt64(e.Message.Channel.Id),
                CreationDate = e.Message.CreationTimestamp.DateTime
            };
            try
            {
                var user = await _context.Users.FindAsync(Convert.ToInt64(e.Message.Author.Id));

                if (user == null)
                {
                    user = new Users
                    {
                        UserId = Convert.ToInt64(e.Author.Id)
                    };

                    _context.Users.Add(user);
                }

                await _context.Messages.AddAsync(msg);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error trying to enter Message into Database");
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Call when Reaction is added to Message
        /// </summary>
        /// <param name="e">
        /// The Event Args.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task ClientMessageReactionAdded(MessageReactionAddEventArgs e)
        {
            await this.tmblrBot.CheckLoli(e);
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

            if (!Directory.Exists(Config.Pornhub.IndexedVideoLocation))
            {
                Directory.CreateDirectory(Config.Pornhub.IndexedVideoLocation);
            }
        }

        private Task Log(string msg)
        {
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }

        private async Task OnReadyAsync(ReadyEventArgs e)
        {
            await Task.Yield();
        }

        private void SetCommands()
        {
            var ccfg = new CommandsNextConfiguration()
            {
                CaseSensitive = false,
                EnableDefaultHelp = false,
                EnableDms = true,
                EnableMentionPrefix = true,
                IgnoreExtraArguments = true,
                StringPrefix = Prefix
            };

            this._context = new DiscordContext();
            this.Commands = this.client.UseCommandsNext(ccfg);

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

            this.client = new DiscordClient(config);

            this.Interactivity = this.client.UseInteractivity(
                new InteractivityConfiguration()
                {
                    PaginationBehaviour = TimeoutBehaviour.Default,
                    PaginationTimeout = TimeSpan.FromSeconds(30),
                    Timeout = TimeSpan.FromSeconds(30)
                });
        }
    }
}