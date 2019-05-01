// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Settings.cs" company="SalemsTools">
//     Do whatever
// </copyright>
// <summary>
// Defines the Settings type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sabrina.Commands
{
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;
    using Sabrina.Entities;
    using Sabrina.Entities.SettingsModules;
    using Sabrina.Models;
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using static Sabrina.Models.UserSettingExtension;

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

        [Command("overview")]
        [Aliases("show")]
        [Description("Show all of your settings")]
        public async Task OverviewAsync(CommandContext ctx)
        {
            var context = new DiscordContext();

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = "These are all of your settings"
            };

            var settings = UserSettingExtension.GetAllSettings(ctx.Message.Author.Id, context).ToDictionary(setting => (UserSettingExtension.SettingID)setting.SettingId);
            var items = await WheelItemExtension.GetUserItemsAsync(ctx.Message.Author.Id, context);

            AnalLevel analLevel = AnalLevel.None;
            if (settings.ContainsKey(SettingID.AnalLevel))
            {
                analLevel = settings[SettingID.AnalLevel].GetValue<AnalLevel>();
            }

            WheelExtension.WheelDifficultyPreference wheelDifficulty = WheelExtension.WheelDifficultyPreference.Default;
            if (settings.ContainsKey(SettingID.WheelDifficulty))
            {
                wheelDifficulty = settings[SettingID.WheelDifficulty].GetValue<WheelExtension.WheelDifficultyPreference>();
            }

            WheelExtension.WheelTaskPreferenceSetting wheelTaskPreference = WheelExtension.WheelTaskPreferenceSetting.Default;
            if (settings.ContainsKey(SettingID.WheelTaskPreference))
            {
                wheelTaskPreference = settings[SettingID.WheelTaskPreference].GetValue<WheelExtension.WheelTaskPreferenceSetting>();
            }

            BondageLevel bondageLevel = BondageLevel.None;
            if (settings.ContainsKey(SettingID.BondageLevel))
            {
                bondageLevel = settings[SettingID.BondageLevel].GetValue<BondageLevel>();
            }

            CBTLevel cbtLevel = CBTLevel.None;
            if (settings.ContainsKey(SettingID.CBTLevel))
            {
                cbtLevel = settings[SettingID.CBTLevel].GetValue<CBTLevel>();
            }

            DungeonDifficulty dungeonDifficulty = DungeonDifficulty.Normal;
            if (settings.ContainsKey(SettingID.DungeonDifficulty))
            {
                dungeonDifficulty = settings[SettingID.DungeonDifficulty].GetValue<DungeonDifficulty>();
            }

            //PeeLevel peeLevel = PeeLevel.None;
            //if (settings.ContainsKey(SettingID.PeeLevel))
            //{
            //    peeLevel = settings[SettingID.PeeLevel].GetValue<PeeLevel>();
            //}

            SissyLevel sissyLevel = SissyLevel.None;
            if (settings.ContainsKey(SettingID.SissyLevel))
            {
                sissyLevel = settings[SettingID.SissyLevel].GetValue<SissyLevel>();
            }

            //DegrationLevel degrationLevel = DegrationLevel.None;
            //if (settings.ContainsKey(SettingID.DegrationLevel))
            //{
            //    degrationLevel = settings[SettingID.DegrationLevel].GetValue<DegrationLevel>();
            //}

            PublicLevel publicLevel = PublicLevel.None;
            if (settings.ContainsKey(SettingID.PublicLevel))
            {
                publicLevel = settings[SettingID.PublicLevel].GetValue<PublicLevel>();
            }

            Gender gender = Gender.None;
            if (settings.ContainsKey(SettingID.Gender))
            {
                gender = settings[SettingID.Gender].GetValue<Gender>();
            }

            foreach (SettingID possibleSetting in Enum.GetValues(typeof(SettingID)))
            {
                Enum setting = null;

                switch (possibleSetting)
                {
                    case SettingID.AnalLevel:
                        setting = analLevel;
                        break;

                    case SettingID.BanType:
                        break;

                    case SettingID.WheelDifficulty:
                        setting = wheelDifficulty;
                        break;

                    case SettingID.WheelTaskPreference:
                        setting = wheelTaskPreference;
                        break;

                    case SettingID.BondageLevel:
                        setting = bondageLevel;
                        break;

                    case SettingID.Toys:
                        break;

                    case SettingID.CBTLevel:
                        setting = cbtLevel;
                        break;

                    case SettingID.DungeonDifficulty:
                        break;

                    case SettingID.PeeLevel:
                        break;

                    case SettingID.SissyLevel:
                        setting = sissyLevel;
                        break;

                    case SettingID.DegrationLevel:
                        break;

                    case SettingID.PublicLevel:
                        setting = publicLevel;
                        break;

                    case SettingID.BanEnd:
                        break;

                    case SettingID.Gender:
                        setting = gender;
                        break;
                }

                if (setting == null)
                {
                    continue;
                }

                var name = possibleSetting.ToFormattedText();
                var value = $"{setting.ToFormattedText()} ({Convert.ToInt32(setting)})";

                builder.AddField(name, value, true);
            }

            await ctx.RespondAsync(embed: builder.Build());

            if (items.Any())
            {
                builder = new DiscordEmbedBuilder()
                {
                    Title = "Your Items"
                };

                foreach (var item in items.OrderBy(it => it.ItemId))
                {
                    var cItem = (WheelItemExtension.Item)item.ItemId;
                    builder.AddField(cItem.ToFormattedText(), item.ItemId.ToString());
                }

                await ctx.RespondAsync(embed: builder.Build());
            }
        }

        /// <summary>
        /// Setup Settings and similar for the current User.
        /// </summary>
        /// <param name="ctx">The Command Context.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        [Command("setup")]
        [Aliases("configure")]
        [Description("Configure your Settings")]
        public async Task SetupAsync(CommandContext ctx)
        {
            await ctx.RespondAsync("You got Mail! *AOL chime plays*");

            var context = new DiscordContext();

            var dm = await ctx.Member.CreateDmChannelAsync();

            var userId = Convert.ToInt64(ctx.User.Id);

            string userName = ctx.Message.Author.Username;
            string readableUserName = RemoveSpecialCharacters(userName);

            if (readableUserName.Length > 4)
            {
                userName = readableUserName;
            }

            await dm.TriggerTypingAsync();
            await Task.Delay(1000);
            await dm.SendMessageAsync($"Hey there {userName}.");
            await Task.Delay(1000);

            bool isFirstChange = true;

            var settings = UserSettingExtension.GetAllSettings(userId, context);

            if (settings == null || settings.Count() == 0)
            {
                // *** First time setup ***

                if (!await Introduction(dm, ctx))
                {
                    return;
                }

                await dm.TriggerTypingAsync();
                await Task.Delay(1000);
                await dm.SendMessageAsync($"Splendid! Now that you know how this works, let's start with the Settings!");

                if (!await new WheelDifficultyModule(context, dm, ctx, userId).Execute())
                {
                    return;
                }

                if (!await new WheelTaskPreferenceModule(context, dm, ctx, userId).Execute())
                {
                    return;
                }

                isFirstChange = false;
            }

            // *** User has come back to change settings ***

            bool exit = false;

            while (!exit)
            {
                await dm.SendMessageAsync($"What{(isFirstChange ? "" : " else")} would you like to change?");

                isFirstChange = false;

                var m = await ctx.Client.GetInteractivityModule().WaitForMessageAsync(
                    x => x.Channel.Id == dm.Id && x.Author.Id == ctx.Member.Id,
                            TimeSpan.FromSeconds(240));

                if (m == null)
                {
                    // *** User didn't enter anything ***
                    return;
                }

                var text = m.Message.Content;

                var modules = ReflectiveEnumerator.GetEnumerableOfType<SettingsModule>(context, dm, ctx, userId).OrderByDescending(module => module.GetScore(text)).ToList();

                int suggestion = 0;
                bool isMatch = false;

                while (suggestion < 2 && !isMatch)
                {
                    await dm.TriggerTypingAsync();
                    await Task.Delay(1000);
                    await dm.SendMessageAsync($"Do you want to change your {modules[suggestion].FriendlyName} settings? (Y/N)");

                    var suggestionReply = await ctx.Client.GetInteractivityModule().WaitForMessageAsync(
                        x => x.Channel.Id == dm.Id && x.Author.Id == ctx.Member.Id,
                                TimeSpan.FromSeconds(240));

                    if (suggestionReply == null)
                    {
                        return;
                    }

                    if (Regex.Match(suggestionReply.Message.Content, YesRegex).Success)
                    {
                        isMatch = true;
                        exit = !await modules[suggestion].Execute();
                    }
                    else
                    {
                        suggestion++;
                    }
                }

                if (suggestion >= 2)
                {
                    DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                    {
                        Title = "These are your options"
                    };

                    foreach (var module in modules)
                    {
                        builder.AddField(module.FriendlyName, "--------", false);
                    }

                    await dm.TriggerTypingAsync();
                    await Task.Delay(2000);
                    await dm.SendMessageAsync(embed: builder.Build());
                }
                else
                {
                    await dm.TriggerTypingAsync();
                    await Task.Delay(1000);
                    await dm.SendMessageAsync("Would you like to change something else? (Y/N)");

                    var moreChangesResponse = await ctx.Client.GetInteractivityModule().WaitForMessageAsync(
                    x => x.Channel.Id == dm.Id && x.Author.Id == ctx.Member.Id,
                            TimeSpan.FromSeconds(240));

                    if (moreChangesResponse == null)
                    {
                        return;
                    }

                    if (Regex.Match(moreChangesResponse.Message.Content, YesRegex).Success)
                    {
                        exit = false;
                    }
                    else
                    {
                        exit = true;
                    }
                }
            }

            await dm.TriggerTypingAsync();
            await Task.Delay(1000);
            await dm.SendMessageAsync($"Alrighty! I'll try to remember that");
            await Task.Delay(2000);
            await dm.TriggerTypingAsync();
            await Task.Delay(500);
            await dm.SendMessageAsync($"..... I'll better write it down...");
            await dm.TriggerTypingAsync();

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await dm.SendMessageAsync($"**Uhh... something seems to have gone badly wrong...{Environment.NewLine}" +
                    $"If you see Salem around here somewhere, tell him the following:**");
                await dm.TriggerTypingAsync();
                await Task.Delay(5000);

                string msgToSend = $"```{ex.Message}```";
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

            await dm.SendMessageAsync($"Done!");

            await dm.TriggerTypingAsync();
            await Task.Delay(1000);
            await dm.SendMessageAsync($"Nice. You can now start using the Wheel with your brand new set of settings \\*-\\*{Environment.NewLine}" +
                                        $"These might get more over time. I will remind you to revisit them, when i feel fit.");
        }

        private static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private async Task<bool> Introduction(DiscordDmChannel dm, CommandContext ctx)
        {
            bool userAgrees = false;

            while (!userAgrees)
            {
                await dm.TriggerTypingAsync();
                await Task.Delay(8000);
                await dm.SendMessageAsync($"I'm gonna send you a bunch of Questions.{Environment.NewLine}" +
                $"Please answer them.{Environment.NewLine}" +
                $"Don't worry, this is not a Quiz. I'll give you enough time.{Environment.NewLine}" +
                $"Most questions will also have possible answers written below. Some may require you, to input a text, some may only require a number.{Environment.NewLine}" +
                $"For every Question, there will be a \"Default\" option. If you don't know what to take / don't have a preference, take this.");

                await dm.TriggerTypingAsync();
                await Task.Delay(12000);

                var builder = new DiscordEmbedBuilder()
                {
                    Title = "Do you understand what i just wrote?",
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
                    return false;
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

            return userAgrees;
        }
    }
}