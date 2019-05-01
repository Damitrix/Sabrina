using DSharpPlus.Entities;
using Sabrina.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Sabrina.Models.UserSettingExtension;

namespace Sabrina.Entities.PunishmentModules
{
    internal class BanModule : PunishmentModule
    {
        public BanModule(Dictionary<UserSettingExtension.SettingID, UserSetting> settings, List<WheelUserItem> items) : base(settings, items)
        {
        }

        public override int Chance { get; internal set; } = 10;
        public override TimeSpan DenialTime { get; internal set; }
        public override DiscordEmbed Embed { get; internal set; }

        public override IEnumerable<SettingID> RequiredSettings { get; internal set; }
        public override TimeSpan WheelLockTime { get; internal set; }

        public override Task Generate()
        {
            BanType previousBan = BanType.None;
            if (_settings.ContainsKey(SettingID.BanType))
            {
                previousBan = _settings[SettingID.BanType].GetValue<BanType>();
            }

            DateTime previousBanEnd = DateTime.Now;
            if (_settings.ContainsKey(SettingID.BanEnd))
            {
                previousBanEnd = _settings[SettingID.BanEnd].GetValue<DateTime>();
            }

            WheelExtension.WheelDifficultyPreference difficulty = WheelExtension.WheelDifficultyPreference.Default;
            if (_settings.ContainsKey(SettingID.WheelDifficulty))
            {
                difficulty = _settings[SettingID.WheelDifficulty].GetValue<WheelExtension.WheelDifficultyPreference>();
            }

            List<BanType> possibleBans = new List<BanType>()
            { BanType.NoHentai, BanType.OnlyHentai, BanType.NoRegularPorn, BanType.NoRegularPorn};

            if (_settings.TryGetValue(SettingID.TrapLevel, out UserSetting sissySetting))
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