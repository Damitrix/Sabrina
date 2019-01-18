// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrgasmWheel.cs" company="SalemsTools">
//   Do whatever
// </copyright>
// <summary>
//   Defines the OrgasmWheel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Configuration;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Sabrina.Entities;
using Sabrina.Entities.Persistent;
using Sabrina.Models;

namespace Sabrina.Commands
{
    /// <summary>
    ///     The orgasm wheel Command Group.
    /// </summary>
    internal class OrgasmWheel
    {
        /// <summary>
        ///     The wheel outcomes.
        /// </summary>
        private List<WheelOutcome> wheelOutcomes;

        /// <summary>
        ///     The add link async.
        /// </summary>
        /// <param name="ctx">
        ///     The Command Context.
        /// </param>
        /// <param name="creator">
        ///     The creator.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="url">
        ///     The url.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("addlink")]
        [Description("Add a Link to the OrgasmWheel")]
        [RequireRolesAttribute("mistress", "techno kitty", "content creator", "trusted creator")]
        public async Task AddLinkAsync(
            CommandContext ctx,
            [Description("The Person who created the Content")]
            string creator,
            [Description("Picture or Video")] string type,
            [Description("The Link")] string url)
        {
            if (Enum.TryParse(type, out Link.ContentType linkType))
            {
                Link link = new Link
                {
                    CreatorName = creator,
                    Type = linkType,
                    Url = url
                };
                link.Save();
                await ctx.RespondAsync("Link added!");
            }
            else
            {
                await ctx.RespondAsync(
                    $"Cannot Parse \"{type}\". Please be sure to use either \"Video\", or \"Picture\".");
            }
        }

        /// <summary>
        ///     The denial time Command.
        /// </summary>
        /// <param name="ctx">
        ///     The Command Context.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("denialtime")]
        [Description("Shows how much longer you should not come")]
        [Aliases("denieduntil")]
        public async Task DenialTimeAsync(CommandContext ctx)
        {
            DiscordContext context = new DiscordContext();

            Users user = await context.Users.FindAsync(Convert.ToInt64(ctx.Message.Author.Id));

            var denialString = "I guess I could give you another chance to cum...";
            var wheelLockedString = "So how about you test your luck?";

            if (user.DenialTime != null && user.DenialTime > DateTime.Now)
            {
                TimeSpan timeLeft = user.DenialTime.Value - DateTime.Now;

                if (timeLeft > TimeSpan.FromHours(24))
                {
                    denialString = "Haha, no, you won't cum today.";
                }
                else if (timeLeft > TimeSpan.FromHours(12))
                {
                    denialString =
                        "Ask me again after you've slept a bit... Or gone to work or whatever, I don't care.";
                }
                else if (timeLeft > TimeSpan.FromHours(6))
                {
                    denialString = "Don't be ridiculous. You won't get a chance to cum now.";
                }
                else if (timeLeft > TimeSpan.FromHours(2))
                {
                    denialString = "Maybe later. I don't feel like you should cum right now.";
                }
                else if (timeLeft > TimeSpan.FromMinutes(20))
                {
                    denialString = "You won't cum right now. How about you try again in... say... 30 minutes? An hour?";
                }
                else
                {
                    denialString = "No, you won't get a chance now. I want to see you squirm for just a few more minutes~";
                }

                if (user.LockTime != null && user.LockTime < DateTime.Now)
                {
                    wheelLockedString = $"But i would allow you to spin right now, if you want {Environment.NewLine}" +
                                        $"*grins* {Environment.NewLine}" +
                                        "There\'s no way I\'ll let you cum though. You didn\'t deserve it yet.";
                }
                else
                {
                    wheelLockedString = "";
                }
            }
            else
            {
                if (user.LockTime != null && user.LockTime > DateTime.Now)
                {
                    TimeSpan lockTimeLeft = user.LockTime.Value - DateTime.Now;

                    if (lockTimeLeft > TimeSpan.FromHours(24))
                    {
                        wheelLockedString = "But not today.";
                    }
                    else if (lockTimeLeft > TimeSpan.FromHours(12))
                    {
                        wheelLockedString = "But i don't want to, right now. Maybe later today.";
                    }
                    else if (lockTimeLeft > TimeSpan.FromHours(6))
                    {
                        wheelLockedString = "But I'm not in the mood right now. Ask me again in a few hours.";
                    }
                    else if (lockTimeLeft > TimeSpan.FromHours(2))
                    {
                        wheelLockedString = "But i'm kinda in the middle of something right now. We can play later.";
                    }
                    else
                    {
                        wheelLockedString = "I won't let you spin for the moment though :)";
                    }
                }
            }

            await ctx.RespondAsync($"Hey {(await ctx.Client.GetUserAsync(Convert.ToUInt64(user.UserId))).Mention},\n" +
                                   $"{denialString}\n" +
                                   $"{wheelLockedString}");
        }

        /// <summary>
        ///     The purge links command.
        /// </summary>
        /// <param name="ctx">
        ///     The Command Context.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("purgelinks")]
        [Description("Purges Links")]
        [RequireOwner]
        public async Task PurgeLinksAsync(CommandContext ctx)
        {
            List<Link> links = await Link.LoadAll();

            List<Link> linksToDelete = new List<Link>();

            foreach (Link origLink in links)
            foreach (Link currentLink in links)
                if (currentLink.FileName != origLink.FileName && currentLink.Url == origLink.Url)
                {
                    linksToDelete.Add(currentLink);
                }

            var outString = string.Empty;

            foreach (Link link in linksToDelete)
            {
                outString += link.FileName + "\n";
                link.Delete();
            }

            await ctx.RespondAsync($"I've deleted the duplicates\n{outString}");
        }

        /// <summary>
        ///     The remove profile async.
        /// </summary>
        /// <param name="ctx">
        ///     The Command Context.
        /// </param>
        /// <param name="dcUser">
        ///     The Discord user.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification =
            "Reviewed. Suppression is OK here.")]
        [Command("resetuser")]
        [Description("Reset a Users saved Data")]
        [Aliases("ru")]
        [RequireRolesAttribute("mistress", "minion", "techno kitty")]
        public async Task RemoveProfileAsync(CommandContext ctx,
            [Description("Mention the user here")] DiscordUser dcUser)
        {
            DiscordContext context = new DiscordContext();
            Users user = await context.Users.FindAsync(Convert.ToInt64(ctx.Message.Author.Id));

            user.DenialTime = null;
            user.BanTime = null;
            user.LockTime = null;
            user.SpecialTime = null;
            user.RuinTime = null;

            await context.SaveChangesAsync();

            await ctx.RespondAsync($"I've reset the Profile of {dcUser.Mention}.");
        }

        /// <summary>
        ///     The show links Command.
        /// </summary>
        /// <param name="ctx">
        ///     The Command Context.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("showlinks")]
        [Description("Shows all Links")]
        [RequireOwner]
        public async Task ShowLinksAsync(CommandContext ctx)
        {
            List<Link> links = await Link.LoadAll();

            var text = "Here are all Links:\n```";

            foreach (Link link in links)
            {
                if ((text + link.Url).Length > 1999)
                {
                    text += "```";
                    await ctx.RespondAsync(text);
                    text = "Here are more Links:\n```";
                }

                text += link.Url + "\n";
            }

            text += "```";

            await ctx.RespondAsync(text);
        }

        /// <summary>
        ///     The orgasm wheel Command.
        /// </summary>
        /// <param name="ctx">
        ///     The Command Context.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Command("orgasmwheel")]
        [Description("Spins a wheel")]
        public async Task SpinNewWheelAsync(CommandContext ctx)
        {
            DiscordContext context = new DiscordContext();
            Users user = await context.Users.FindAsync(Convert.ToInt64(ctx.Message.Author.Id));
            UserSettings userSettings = await context.UserSettings.FindAsync(Convert.ToInt64(ctx.Message.Author.Id));
            SabrinaSettings sabrinaSettings = await context.SabrinaSettings.FindAsync(Convert.ToInt64(ctx.Guild.Id));

            if (sabrinaSettings == null)
            {
                sabrinaSettings = new SabrinaSettings
                {
                    GuildId = Convert.ToInt64(ctx.Guild.Id),
                    WheelChannel = Convert.ToInt64(ctx.Channel.Id)
                };

                await context.SabrinaSettings.AddAsync(sabrinaSettings);
                await context.SaveChangesAsync();
            }

            if (sabrinaSettings.WheelChannel == null)
            {
                sabrinaSettings.WheelChannel = Convert.ToInt64(ctx.Channel.Id);
                await context.SaveChangesAsync();
            }

            if (Convert.ToInt64(ctx.Channel.Id) != sabrinaSettings.WheelChannel.Value)
            {
                DiscordChannel channel =
                    await ctx.Client.GetChannelAsync(Convert.ToUInt64(sabrinaSettings.WheelChannel));

                await ctx.RespondAsync(
                    $"You cannot issue this command from this Channel. Please use {channel.Mention}");
                return;
            }

            if (user == null)
            {
                user = new Users();
            }

            if (userSettings == null)
            {
                userSettings = new UserSettings();
            }

            WheelChances chances = await context.WheelChances.FindAsync(userSettings.WheelDifficulty ?? 2);

            if (user.LockTime != null && user.LockTime > DateTime.Now)
            {
                TimeSpan? timeUntilFree = user.LockTime - DateTime.Now;

                TimeSpan newTimeUntilFree =
                    TimeSpan.FromTicks(timeUntilFree.Value.Ticks * Helpers.RandomGenerator.RandomInt(1, 4));

                if (newTimeUntilFree > TimeSpan.FromDays(365))
                {
                    await ctx.RespondAsync("Fuck off");
                    return;
                }

                var responseText = "Oho, it seems like I told you to stay away from spinning the wheel...\n" +
                                   $"That means you get some more extra time of no spinning {DiscordEmoji.FromName(ctx.Client, Config.Emojis.Blush)}";
                
                if (int.TryParse(user.LockReason, out int reasonInt))
                {
                    var reason = (UserSettingsExtension.LockReason) reasonInt;

                    switch (reason)
                    {
                            case UserSettingsExtension.LockReason.Cooldown:
                                responseText =
                                    "Oho, it seems like I told you to stay away from spinning the wheel...\n" +
                                    $"That means you get some more extra time of no spinning {DiscordEmoji.FromName(ctx.Client, Config.Emojis.Blush)}";
                                user.LockTime += newTimeUntilFree;
                            break;
                            case UserSettingsExtension.LockReason.Extension:
                                responseText =
                                    "Hey, i already told you, that you'd get a longer lock on the Wheel! You still want more? Sure!";
                                user.LockTime += TimeSpan.FromMinutes(1);
                            break;
                            case UserSettingsExtension.LockReason.Task:
                                responseText = "Haha, there's no way you were able to finish your Task so quickly. Do your Task, " +
                                               "and then I'll give you another minute to think about your Actions.";
                                user.LockTime += TimeSpan.FromMinutes(1);
                            break;
                    }
                }

                user.LockReason = ((int)UserSettingsExtension.LockReason.Extension).ToString();

                await ctx.RespondAsync(responseText);

                await context.SaveChangesAsync().ConfigureAwait(false);

                return;
            }

            var addedChance = 0;

            var outcomeChanceValue = Helpers.RandomGenerator.RandomInt(
                0,
                chances.Denial + chances.Task + chances.Edge + chances.Ruin + chances.Orgasm);
            Console.WriteLine("Chance Rolled: " + outcomeChanceValue);
            Console.WriteLine("Denial Chance: " + chances.Denial);
            Console.WriteLine("Edge Chance: " + chances.Edge);
            Console.WriteLine("TaskChance Chance: " + chances.Task);
            Console.WriteLine("RuinChance Chance: " + chances.Ruin);
            Console.WriteLine("OrgasmChance Chance: " + chances.Orgasm);
            Console.WriteLine("addedChance Chance: " + addedChance);
            WheelExtension.Outcome outcome = WheelExtension.Outcome.Edge;
            if (outcomeChanceValue < chances.Denial)
            {
                outcome = WheelExtension.Outcome.Denial;
            }
            else if (outcomeChanceValue < chances.Denial + chances.Edge)
            {
                outcome = WheelExtension.Outcome.Edge;
            }
            else if (outcomeChanceValue < chances.Denial + chances.Edge + chances.Task)
            {
                outcome = WheelExtension.Outcome.Task;
            }
            else if (outcomeChanceValue < chances.Denial + chances.Edge + chances.Task + chances.Ruin)
            {
                outcome = WheelExtension.Outcome.Ruin;
            }
            else
            {
                outcome = WheelExtension.Outcome.Orgasm;
            }

            Console.WriteLine("1");

            if (user.DenialTime != null && user.DenialTime > DateTime.Now)
            {
                TimeSpan? timeUntilFree = user.DenialTime - DateTime.Now;

                if (outcome.HasFlag(WheelExtension.Outcome.Orgasm)
                    || outcome.HasFlag(WheelExtension.Outcome.Ruin))
                {
                    await ctx.RespondAsync(
                        "Haha, I would\'ve let you cum this time, but since you\'re still denied, "
                        + $"that won't happen {DiscordEmoji.FromName(ctx.Client, Config.Emojis.Blush)}.\n" +
                        "As a punishment, you\'re gonna do your Task anyways though:");
                }
                else
                {
                    await ctx.RespondAsync(
                        "Well, i told you, that you\'d be denied now.\n"
                        + "You still want to do something? Hmm... let's see...");
                }

                await Task.Delay(1500);
                outcome = WheelExtension.Outcome.Denial | WheelExtension.Outcome.Edge;
            }

            Console.WriteLine("2");

            WheelOutcome wheelOutcome = null;

            while (wheelOutcome == null)
            {
                wheelOutcomes = ReflectiveEnumerator.GetEnumerableOfType<WheelOutcome>(outcome, userSettings, context)
                    .ToList();

                wheelOutcomes = wheelOutcomes.Where(e => outcome.HasFlag(e.Outcome)).ToList();

                if (wheelOutcomes.Count < 1)
                {
                    continue;
                }

                // Choose an outcome by summing up the chance values of all possible outcomes and then generating a random number inside those.
                var combinedChance = 0;

                foreach (WheelOutcome currentOutcome in wheelOutcomes) combinedChance += currentOutcome.Chance;

                var chance = 0;
                var minChance = Helpers.RandomGenerator.RandomInt(0, combinedChance);

                foreach (WheelOutcome currentOutcome in wheelOutcomes)
                {
                    chance += currentOutcome.Chance;
                    if (minChance < chance)
                    {
                        wheelOutcome = currentOutcome;
                        break;
                    }
                }
            }

            Console.WriteLine("3");

            if (user.DenialTime == null)
            {
                user.DenialTime = DateTime.Now;
            }

            if (user.LockTime == null)
            {
                user.LockTime = DateTime.Now;
            }

            if (user.DenialTime < DateTime.Now)
            {
                user.DenialTime = DateTime.Now;
            }

            if (user.LockTime < DateTime.Now)
            {
                user.LockTime = DateTime.Now;
            }

            if (wheelOutcome.WheelLockedTime != null && wheelOutcome.WheelLockedTime > TimeSpan.Zero)
            {
                switch (outcome)
                {
                    case WheelExtension.Outcome.Edge:
                    case WheelExtension.Outcome.Task:
                        user.LockReason = ((int)UserSettingsExtension.LockReason.Task).ToString();
                        break;
                    default:
                        user.LockReason = ((int)UserSettingsExtension.LockReason.Cooldown).ToString();
                        break;
                }
            }

            user.DenialTime += wheelOutcome.DenialTime;
            user.LockTime += wheelOutcome.WheelLockedTime;
            await context.SaveChangesAsync();

            Console.WriteLine("4");

            if (wheelOutcome.Embed != null)
            {
                await ctx.RespondAsync(embed: wheelOutcome.Embed);
            }
            else
            {
                await ctx.RespondAsync(wheelOutcome.Text);
            }
        }

        /// <summary>
        ///     The spin wheel Command.
        /// </summary>
        /// <param name="ctx">
        ///     The Command Context.
        /// </param>
        /// <returns>
        ///     A <see cref="Task" />.
        /// </returns>
        [Command("oldorgasmwheel")]
        [Description("Spins the old wheel. Just in case the new one is broken.")]
        [Aliases("orgasmwheel1", "orgasmwheel2")]
        public async Task SpinWheelAsync(CommandContext ctx)
        {
            if (ctx.Channel.Name != Config.Channels.Wheel)
            {
                return;
            }

            var outcome = Helpers.RandomGenerator.RandomInt(0, 100);

            var line = "Sorry, no Entries yet.";
            if (outcome < 92)
            {
                line = await LoadLineAsync($"{Config.BotFileFolders.WheelResponses}/Denial.txt");
            }
            else if (outcome < 96)
            {
                line = await LoadLineAsync($"{Config.BotFileFolders.WheelResponses}/Ruin.txt");
            }
            else
            {
                line = await LoadLineAsync($"{Config.BotFileFolders.WheelResponses}/Orgasm.txt");
            }

            await ctx.RespondAsync(line);
        }

        /// <summary>
        ///     Loads a random Line from a TextDocument
        /// </summary>
        /// <param name="file">
        ///     The file path.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        private async Task<string> LoadLineAsync(string file)
        {
            using (StreamReader reader = File.OpenText(file))
            {
                var fileText = await reader.ReadToEndAsync();
                string[] lines = fileText.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
                return lines[Helpers.RandomGenerator.RandomInt(0, lines.Length)];
            }
        }
    }
}