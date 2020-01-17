using DSharpPlus.Entities;
using Sabrina.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Sabrina.Models.UserSetting;

namespace Sabrina.Entities.PunishmentModules
{
    internal class BanModule : PunishmentModule
    {
        public BanModule(Dictionary<UserSetting.SettingID, UserSetting> settings, List<WheelUserItem> items) : base(settings, items)
        {
        }

        public override int Chance { get; internal set; } = 10;
        public override TimeSpan DenialTime { get; internal set; }
        public override DiscordEmbed Embed { get; internal set; }
        public override TimeSpan WheelLockTime { get; internal set; }

        public override Task Generate()
        {
            BanType previousBan = BanType.None;
            if (Settings.ContainsKey(SettingID.BanType))
            {
                previousBan = Settings[SettingID.BanType].GetValue<BanType>();
            }

            DateTime previousBanEnd = DateTime.Now;
            if (Settings.ContainsKey(SettingID.BanEnd))
            {
                previousBanEnd = Settings[SettingID.BanEnd].GetValue<DateTime>();
            }

            UserSetting.WheelDifficultyPreference difficulty = UserSetting.WheelDifficultyPreference.Default;
            if (Settings.ContainsKey(SettingID.WheelDifficulty))
            {
                difficulty = Settings[SettingID.WheelDifficulty].GetValue<UserSetting.WheelDifficultyPreference>();
            }

            List<BanType> possibleBans = new List<BanType>()
            { BanType.NoHentai, BanType.OnlyHentai, BanType.NoRegularPorn, BanType.NoRegularPorn};

            if (Settings.TryGetValue(SettingID.TrapLevel, out UserSetting sissySetting))
            {
                if (sissySetting.GetValue<SissyLevel>() >= SissyLevel.Normal)
                {
                    possibleBans.Add(BanType.OnlyTrapPorn);
                }
            }

            var index = Helpers.RandomGenerator.RandomInt(0, possibleBans.Count);
            var chosenBan = possibleBans[index];

            int days = (int)difficulty + 1;

            var builder = new DiscordEmbedBuilder()
            {
                Title = "Ban!",
                Description = $"You are allowed to watch {chosenBan.ToFormattedText().ToLower()} for the next {days} days."
            };

            if (previousBan != BanType.None && previousBanEnd > DateTime.Now)
            {
                builder.Description += $" Your previous ban of {previousBan.ToFormattedText().ToLower()} has been lifted.";
            }

            Embed = builder.Build();

            return Task.CompletedTask;
        }
    }
}