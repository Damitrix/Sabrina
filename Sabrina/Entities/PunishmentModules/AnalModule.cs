using DSharpPlus.Entities;
using Sabrina.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sabrina.Models.UserSetting;

namespace Sabrina.Entities.PunishmentModules
{
    internal class AnalModule : PunishmentModule
    {
        public AnalModule(Dictionary<UserSetting.SettingID, UserSetting> settings, List<WheelUserItem> items) : base(settings, items)
        {
            AnalLevel analLevel = AnalLevel.None;
            if (Settings.ContainsKey(SettingID.AnalLevel))
            {
                analLevel = Settings[SettingID.AnalLevel].GetValue<AnalLevel>();
            }

            List<WheelUserItem> toys = Items.Where(item => WheelItemExtension.GetItemCategory((WheelItemExtension.Item)item.ItemId) == WheelItemExtension.Item.Toy).ToList();

            if (analLevel == AnalLevel.None || !toys.Any())
            {
                Chance = 0;

                if (!Settings.ContainsKey(SettingID.AnalLevel))
                {
                    ((List<UserSetting.SettingID>)RequiredSettings).Add(UserSetting.SettingID.AnalLevel);
                }
            }
        }

        public override int Chance { get; internal set; } = 50;
        public override TimeSpan DenialTime { get; internal set; }
        public override DiscordEmbed Embed { get; internal set; }
        public override TimeSpan WheelLockTime { get; internal set; }

        public override Task Generate()
        {
            UserSetting.WheelDifficultyPreference difficulty = UserSetting.WheelDifficultyPreference.Default;
            if (Settings.ContainsKey(SettingID.WheelDifficulty))
            {
                difficulty = Settings[SettingID.WheelDifficulty].GetValue<UserSetting.WheelDifficultyPreference>();
            }

            AnalLevel analLevel = AnalLevel.None;
            if (Settings.ContainsKey(SettingID.AnalLevel))
            {
                analLevel = Settings[SettingID.AnalLevel].GetValue<AnalLevel>();
            }

            List<WheelUserItem> toys = Items.Where(item => WheelItemExtension.GetItemCategory((WheelItemExtension.Item)item.ItemId) == WheelItemExtension.Item.Toy).ToList();

            var toy = (WheelItemExtension.Item)toys[Helpers.RandomGenerator.RandomInt(0, toys.Count)].ItemId;

            var builder = new DiscordEmbedBuilder()
            {
                Title = "Backside fun"
            };

            int time = (int)difficulty * (int)analLevel;
            time = Helpers.RandomGenerator.RandomInt(time / 3 * 2, time / 3 * 4);

            switch (Helpers.RandomGenerator.RandomInt(0, 3))
            {
                case 0:
                    string vibeText = "";

                    if (toy == WheelItemExtension.Item.Vibrator)
                    {
                        string vibeSpeed = "";

                        switch (analLevel)
                        {
                            case AnalLevel.Light:
                                vibeSpeed = "lowest";
                                break;

                            case AnalLevel.Normal:
                                vibeSpeed = "normal";
                                break;

                            case AnalLevel.Hardcore:
                                vibeSpeed = "highest";
                                break;
                        }

                        vibeText = $"Turn it on to the {vibeSpeed} setting.";
                    }

                    builder.Description = $"Put your {toy.ToFormattedText()} in your Ass. {vibeText}" + Environment.NewLine
                        + $"Leave it there for the next {time * 2} minutes.";
                    break;

                case 1:
                    builder.Description = $"Take your {toy.ToFormattedText()}, lube it up, then shove it up your butt as fast as you can. Then take it out and give it a kiss." + Environment.NewLine
                        + $"Do this {time} times.";
                    break;

                case 2:
                    builder.Description = $"Take your {toy.ToFormattedText()}, lube it up, and smack your ass with it." + Environment.NewLine
                        + $"Do this {time} times. No cleaning up after that.";
                    break;
            }

            Embed = builder.Build();

            return Task.CompletedTask;
        }
    }
}