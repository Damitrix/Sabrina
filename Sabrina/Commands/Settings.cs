// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Settings.cs" company="SalemsTools">
//   Do whatever
// </copyright>
// <summary>
//   Defines the Settings type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sabrina.Commands
{
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;
    using Sabrina.Models;
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    /// <summary>
    /// The settings.
    /// </summary>
    [Group("settings")]
    [Aliases("setting")]
    internal class Settings
    {
        /// <summary>
        /// The confirm regex.
        /// </summary>
        private const string ConfirmRegex = "\\b[Yy][Ee]?[Ss]?\\b|\\b[Nn][Oo]?\\b";

        /// <summary>
        /// The no regex.
        /// </summary>
        private const string NoRegex = "[Nn][Oo]?";

        /// <summary>
        /// The yes regex.
        /// </summary>
        private const string YesRegex = "[Yy][Ee]?[Ss]?";

        private DiscordContext _context;

        public Settings()
        {
            _context = new DiscordContext();
        }

        /// <summary>
        /// Setup Settings and similar for the current User.
        /// </summary>
        /// <param name="ctx">
        /// The Command Context.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/>.
        /// </returns>
        [Command("setup")]
        [Aliases("configure")]
        [Description("Configure your Settings")]
        public async Task SetupAsync(CommandContext ctx)
        {
            await ctx.RespondAsync("You got Mail! *AOL chime plays*");

            var dm = await ctx.Member.CreateDmChannelAsync();

            await dm.SendMessageAsync($"Hey there {ctx.Message.Author.Username}.");
            await Task.Delay(6000);
            await dm.TriggerTypingAsync();
            await Task.Delay(3000);
            await dm.SendMessageAsync($"I see you want to change your Settings, so here is how this is gonna go.{Environment.NewLine}It's really simple, so don't worry.");
            await Task.Delay(1000);
            await dm.TriggerTypingAsync();
            await Task.Delay(3000);

            bool userAgrees = false;

            while (!userAgrees)
            {
                await dm.SendMessageAsync($"I'm gonna send you a bunch of Questions.{Environment.NewLine}" +
                $"Please answer them.{Environment.NewLine}" +
                $"Don't worry, this is not a Quiz. I'll give you enough time.{Environment.NewLine}" +
                $"Each Question will also have possible answers written below. Some may require you, to input a text, some may only require a number.{Environment.NewLine}" +
                $"For every Question, there will be a \"Default\" option. If you don't know what to take / don't have a preference, take this.");

                await dm.TriggerTypingAsync();
                await Task.Delay(12000);

                var builder = new DiscordEmbedBuilder()
                {
                    Title = "Do you understand?",
                    Url = "http://IJustWantThisToBeBlue.com"
                };

                builder.AddField("I understand. Please go on.", "Yes");
                builder.AddField("What?", "No");

                await dm.SendMessageAsync(embed: builder.Build());

                var m = await ctx.Client.GetInteractivityModule().WaitForMessageAsync(
                    x => x.Channel.Id == dm.Id && x.Author.Id == ctx.Member.Id
                                               && Regex.IsMatch(x.Content, ConfirmRegex),
                            TimeSpan.FromSeconds(240));

                if (m == null)
                {
                    await dm.SendMessageAsync($"Or just don't respond at all. That's ok too :(");
                    return;
                }

                if (Regex.IsMatch(m.Message.Content, YesRegex))
                {
                    userAgrees = true;
                }
                else
                {
                    await dm.SendMessageAsync($"Ok, let me explain it to you.{Environment.NewLine}" +
                        $"I've just sent you a Question. Directly underneath it, you can see a fat \"**I understand**\", and a smaller \"Yes\". Right?{Environment.NewLine}" +
                        $"The \"**I understand**\", is basically just a Description of your possible answer. In this case, the Answer is \"Yes\".{Environment.NewLine}" +
                        $"So you can either answer exactly with \"Yes\", or exactly with \"No\".{Environment.NewLine}" +
                        $"So let's try this again.");
                    await dm.TriggerTypingAsync();
                    await Task.Delay(10000);
                }
            }

            await dm.SendMessageAsync($"Splendid! Now that you know how this works, let's start with the Settings! Just Gimi a second to check if you already have some saved.");
            await dm.TriggerTypingAsync();

            var userSettings = _context.UserSettings.Find(Convert.ToInt64(ctx.User.Id));

            if (userSettings == null)
            {
                userSettings = new UserSettings();
                userSettings.UserId = Convert.ToInt64(Convert.ToInt64(ctx.Message.Author.Id));
            }

            await Task.Delay(1000);
            await dm.SendMessageAsync($"There we go. First Question!");
            await dm.TriggerTypingAsync();
            await Task.Delay(4000);

            int? wheelDifficulty = null;

            while (!wheelDifficulty.HasValue)
            {
                var builder = new DiscordEmbedBuilder()
                {
                    Title = "How Difficult would you like the Wheel to be? Lower Difficulties will lower your required Edges and work, but also your Chance for a good ending :)",
                    Url = "http://IJustWantThisToBeBlue.com"
                };

                builder.AddField("Easiest Setting. Almost no Edges, will leave you in ruins.", WheelExtension.WheelDifficultyPreference.Baby.ToString());
                builder.AddField(
                    "Easy Setting, for when you are just starting with Edging.",
                    WheelExtension.WheelDifficultyPreference.Easy.ToString());
                builder.AddField(
                    "Default. This is how the Wheel was before the Settings arrived, and how it is before you set up the settings.",
                    WheelExtension.WheelDifficultyPreference.Default.ToString());
                builder.AddField("Pretty Challenging.", WheelExtension.WheelDifficultyPreference.Hard.ToString());
                builder.AddField("This will make every single roll Hardcore. High risk, High reward though.", WheelExtension.WheelDifficultyPreference.Masterbater.ToString());

                await dm.SendMessageAsync(embed: builder.Build());

                var m = await ctx.Client.GetInteractivityModule().WaitForMessageAsync(
                    x => x.Channel.Id == dm.Id
                         && x.Author.Id == ctx.Member.Id,
                    TimeSpan.FromSeconds(240));

                if (m == null)
                {
                    await dm.SendMessageAsync($"Or just don't respond at all. That's ok too :(");
                    return;
                }

                if (Enum.TryParse(m.Message.Content, out WheelExtension.WheelDifficultyPreference wheelDifficultySetting))
                {
                    wheelDifficulty = (int)wheelDifficultySetting;
                }
                else
                {
                    await dm.SendMessageAsync($"You have to precisely enter the name of one of the Difficulty.");
                }
            }

            await dm.TriggerTypingAsync();
            await Task.Delay(1000);
            await dm.SendMessageAsync($"Ok. Next Question!");
            await dm.TriggerTypingAsync();
            await Task.Delay(2000);

            userSettings.WheelDifficulty = wheelDifficulty;

            WheelExtension.WheelTaskPreferenceSetting? wheelTaskPreference = null;

            while (wheelTaskPreference == null)
            {
                var builder = new DiscordEmbedBuilder()
                {
                    Title = "What kind of task do you prefer? There are no penalties here.",
                    Url = "http://IJustWantThisToBeBlue.com"
                };

                builder.AddField("Edge for 15 Minutes. 30 second Cooldown.", WheelExtension.WheelTaskPreferenceSetting.Time.ToString());
                builder.AddField("Edge 10 times.", WheelExtension.WheelTaskPreferenceSetting.Amount.ToString());
                builder.AddField("Edge 10 times per day, for the next 2 Days.", WheelExtension.WheelTaskPreferenceSetting.Task.ToString());
                builder.AddField("No preference", WheelExtension.WheelTaskPreferenceSetting.Default.ToString());

                await dm.SendMessageAsync(embed: builder.Build());

                var m = await ctx.Client.GetInteractivityModule().WaitForMessageAsync(
                    x => x.Channel.Id == dm.Id
                         && x.Author.Id == ctx.Member.Id,
                    TimeSpan.FromSeconds(240));

                if (m == null)
                {
                    await dm.SendMessageAsync($"Or just don't respond at all. That's ok too :(");
                    return;
                }

                if (Enum.TryParse(m.Message.Content, out WheelExtension.WheelTaskPreferenceSetting wheelPreferenceSetting))
                {
                    wheelTaskPreference = wheelPreferenceSetting;
                }
                else
                {
                    await dm.SendMessageAsync($"You have to precisely enter the name of one of the possible Difficulties.");
                }
            }

            userSettings.WheelTaskPreference = (int)wheelTaskPreference;

            await dm.TriggerTypingAsync();
            await Task.Delay(1000);
            await dm.SendMessageAsync($"That's it for now already! Let me just save that real quick...");
            await dm.TriggerTypingAsync();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await dm.SendMessageAsync($"**Uhh... something seems to have gone badly wrong here...{Environment.NewLine}" +
                    $"If you see Salem around here somewhere, tell him the following:**");
                await dm.TriggerTypingAsync();
                await Task.Delay(5000);

                string msgToSend = ex.Message;
                while (msgToSend.Length > 1999)
                {
                    await dm.SendMessageAsync(msgToSend.Substring(0, 1999));
                    await dm.TriggerTypingAsync();
                    await Task.Delay(2000);
                    msgToSend = msgToSend.Substring(1999);
                }

                await dm.SendMessageAsync(msgToSend);

                return;
            }

            await dm.SendMessageAsync($"Saved!");

            await dm.TriggerTypingAsync();
            await Task.Delay(1000);
            await dm.SendMessageAsync($"Nice. You can now start using the Wheel with your brand new set of settings \\*-\\*{Environment.NewLine}" +
                                        $"These might get more over time. I will remind you to revisit them, when it's time.");
        }
    }
}