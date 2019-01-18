using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using Sabrina.Dungeon;
using Sabrina.Dungeon.Rooms;
using Sabrina.Entities;
using Sabrina.Models;

namespace Sabrina.Commands
{
    [Group("dungeon")]
    internal class Dungeon
    {
        private readonly DiscordContext _context;

        public Dungeon()
        {
            _context = new DiscordContext();
        }

        [Command("continueadvanced")]
        public async Task ContinueLength(CommandContext ctx, string length)
        {
            DungeonSession session =
                _context.DungeonSession.FirstOrDefault(ds => ds.UserId == Convert.ToInt64(ctx.User.Id));

            DungeonLogic.Dungeon dungeon = null;
            Room room = null;

            DungeonLogic.Dungeon.DungeonDifficulty dungeonDifficulty = DungeonLogic.Dungeon.DungeonDifficulty.Medium;
            int? difficulty = (await _context.UserSettings.FindAsync(Convert.ToInt64(ctx.User.Id))).DungeonDifficulty;

            if (difficulty != null)
            {
                dungeonDifficulty = (DungeonLogic.Dungeon.DungeonDifficulty) difficulty.Value;
            }

            if (session != null)
            {
                // Pre-Existing Session found
                dungeon = JsonConvert.DeserializeObject<DungeonLogic.Dungeon>(session.DungeonData);
                room = dungeon.Rooms.First(r => r.RoomID == Guid.Parse(session.RoomGuid));
            }
            else
            {
                DungeonLogic.Dungeon.DungeonLength dungeonLength =
                    (DungeonLogic.Dungeon.DungeonLength) Enum.Parse(typeof(DungeonLogic.Dungeon.DungeonLength), length);

                // Start new Session
                dungeon = new DungeonLogic.Dungeon(1, dungeonLength, dungeonDifficulty);
                room = dungeon.Rooms.First();

                try
                {
                    var dungeonJson = JsonConvert.SerializeObject(dungeon, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });

                    session = new DungeonSession
                    {
                        DungeonData = dungeonJson,
                        UserId = Convert.ToInt64(ctx.User.Id),
                        RoomGuid = room.RoomID.ToString()
                    };

                    await _context.DungeonSession.AddAsync(session);

                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw ex;
                }
            }

            room.SetDifficulty(dungeonDifficulty);

            //Enter Room Message
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Grayple,
                Description = room.GetText(DungeonTextExtension.TextType.RoomEnter)
            };

            await ctx.RespondAsync(embed: builder.Build());

            await ctx.TriggerTypingAsync();
            await Task.Delay(room.WaitAfterMessage);

            if (room.Type == DungeonTextExtension.RoomType.LesserMob || room.Type == DungeonTextExtension.RoomType.Boss)
            {
                //Greeting Message
                builder = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Chartreuse,
                    Description = room.GetText(DungeonTextExtension.TextType.Greeting)
                };

                await ctx.RespondAsync(embed: builder.Build());

                await ctx.TriggerTypingAsync();
                await Task.Delay(room.WaitAfterMessage);
            }

            // Main Message (Task or Chest opening f.e.)
            builder = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Blue,
                Description = room.GetText(DungeonTextExtension.TextType.Main)
            };

            await ctx.RespondAsync(embed: builder.Build());

            await Task.Delay(room.WaitAfterMessage);

            if (room.Type == DungeonTextExtension.RoomType.Loot)
            {
                // TODO: Save Loot
                builder = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Chartreuse,
                    Description = "The Items have been placed into your current Inventory."
                };

                await ctx.RespondAsync(embed: builder.Build());

                await ctx.TriggerTypingAsync();
                await Task.Delay(room.WaitAfterMessage);
            }
            else if (room.Type == DungeonTextExtension.RoomType.LesserMob)
            {
                await ctx.RespondAsync("Did you finish my Task?");
                MessageContext m = await ctx.Client.GetInteractivityModule().WaitForMessageAsync(
                    x => x.Channel.Id == ctx.Channel.Id && x.Author.Id == ctx.Member.Id
                                                        && Regex.IsMatch(x.Content, Helpers.RegexHelper.ConfirmRegex),
                    TimeSpan.FromMilliseconds(room.WaitAfterMessage / 4));

                if (m == null)
                {
                    await ctx.RespondAsync("Well, time\'s up.");
                    // TODO: On Loose

                    // Loose Message
                    builder = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Red,
                        Description = room.GetText(DungeonTextExtension.TextType.Failure)
                    };

                    await ctx.RespondAsync(embed: builder.Build());

                    await ctx.TriggerTypingAsync();
                    await Task.Delay(room.WaitAfterMessage);
                    
                }

                // If Task Successful
                if (Regex.IsMatch(m.Message.Content, Helpers.RegexHelper.YesRegex))
                {
                    // Win Message
                    builder = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Chartreuse,
                        Description = room.GetText(DungeonTextExtension.TextType.Success)
                    };

                    await ctx.RespondAsync(embed: builder.Build());

                    await ctx.TriggerTypingAsync();
                    await Task.Delay(room.WaitAfterMessage);
                }

                // If Task failed
                else if (Regex.IsMatch(m.Message.Content, Helpers.RegexHelper.NoRegex))
                {
                    // TODO: On Loose
                    // Loose Message
                    builder = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Red,
                        Description = room.GetText(DungeonTextExtension.TextType.Failure)
                    };

                    await ctx.RespondAsync(embed: builder.Build());

                    await ctx.TriggerTypingAsync();
                    await Task.Delay(room.WaitAfterMessage);
                }
            }

            if (room.AdjacentRooms == null || room.AdjacentRooms.Length == 0)
            {
                // TODO: Last Room. End Dungeon
            }

            await _context.SaveChangesAsync();
        }

        [Command("continue")]
        public async Task ContinueRandom(CommandContext ctx)
        {
            // This will Start a Session with of random Length - for some extra xp

            Random rnd = new Random();
            var length =
                rnd.Next(Enum.GetNames(typeof(DungeonLogic.Dungeon.DungeonLength)).Length - 1); //Don't include Endless

            await ContinueLength(ctx, Enum.GetNames(typeof(DungeonLogic.Dungeon.DungeonLength))[length]);
        }

        [Command("setdifficulty")]
        public async Task SetDifficulty(CommandContext ctx, string difficulty)
        {
            // Sets the difficulty when in a save zone
            DungeonSession session =
                _context.DungeonSession.FirstOrDefault(ds => ds.UserId == Convert.ToInt64(ctx.User.Id));

            if (session == null)
            {
                _context.UserSettings.Find(Convert.ToInt64(ctx.User.Id)).DungeonDifficulty =
                    (int) Enum.Parse(typeof(UserSettingsExtension.DungeonDifficulty), difficulty);
                await _context.SaveChangesAsync();
            }
        }
    }
}