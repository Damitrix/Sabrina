using DSharpPlus.Entities;
using Sabrina.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sabrina.Models.UserSettingExtension;

namespace Sabrina.Entities.PunishmentModules
{
    internal class SissyModule : PunishmentModule
    {
        public SissyModule(Dictionary<UserSettingExtension.SettingID, UserSetting> settings, List<WheelUserItem> items) : base(settings, items)
        {
            SissyLevel sissyLevel = SissyLevel.None;
            if (_settings.ContainsKey(SettingID.SissyLevel))
            {
                sissyLevel = _settings[SettingID.SissyLevel].GetValue<SissyLevel>();
            }

            if (sissyLevel == SissyLevel.None)
            {
                Chance = 0;

                ((List<UserSettingExtension.SettingID>)RequiredSettings).Add(UserSettingExtension.SettingID.SissyLevel);
            }
        }

        public override int Chance { get; internal set; } = 10;
        public override TimeSpan DenialTime { get; internal set; }
        public override DiscordEmbed Embed { get; internal set; }

        public override IEnumerable<UserSettingExtension.SettingID> RequiredSettings { get; internal set; }
        public override TimeSpan WheelLockTime { get; internal set; }

        public override Task Generate()
        {
            SissyLevel sissyLevel = SissyLevel.None;
            if (_settings.ContainsKey(SettingID.SissyLevel))
            {
                sissyLevel = _settings[SettingID.SissyLevel].GetValue<SissyLevel>();
            }

            PublicLevel publicLevel = PublicLevel.None;
            if (_settings.ContainsKey(SettingID.PublicLevel))
            {
                publicLevel = _settings[SettingID.PublicLevel].GetValue<PublicLevel>();
            }

            WheelExtension.WheelDifficultyPreference difficulty = WheelExtension.WheelDifficultyPreference.Default;
            if (_settings.ContainsKey(SettingID.WheelDifficulty))
            {
                difficulty = _settings[SettingID.WheelDifficulty].GetValue<WheelExtension.WheelDifficultyPreference>();
            }

            List<WheelUserItem> clothing = _items.Where(item => WheelItemExtension.GetItemCategory((WheelItemExtension.Item)item.ItemId) == WheelItemExtension.Item.Clothing).ToList();

            int maxItemCount = Convert.ToInt32((float)difficulty / 2 * (float)sissyLevel);

            maxItemCount = maxItemCount < 1 ? 1 : maxItemCount;

            int itemCount = Helpers.RandomGenerator.RandomInt(1, maxItemCount);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = "Hmm, i bet you'd look cute in something else...",
                Description = "I want you, to put some of your clothes on. In fact, you'll dress up with your "
            };

            for (int i = itemCount; i > 0; i--)
            {
                if (clothing.Count == 0)
                {
                    break;
                }

                var item = clothing[Helpers.RandomGenerator.RandomInt(0, clothing.Count)];

                string postfix = "";

                switch (i)
                {
                    case 2:
                        postfix = " and ";
                        break;

                    case 1:
                        postfix = ". ";
                        break;

                    default:
                        postfix = ", ";
                        break;
                }

                builder.Description += ((WheelItemExtension.Item)item.ItemId).ToFormattedText() + postfix;

                clothing.Remove(item);
            }

            if (sissyLevel >= SissyLevel.Normal)
            {
                switch (publicLevel)
                {
                    case PublicLevel.None:
                    case PublicLevel.Light:
                        builder.Description += "When you're done, leave on everything you're comfortable with for at least 2 hours.";
                        break;

                    case PublicLevel.Normal:
                        builder.Description += "When you're done, leave on everything you're comfortable with for at least 4 hours.";
                        break;

                    case PublicLevel.Hardcore:
                        builder.Description += "When you're done, leave on everything for at least 6 hours.";
                        break;
                }
            }
            else
            {
                builder.Description += "You can take everything off when you're done with your tasks.";
            }

            Embed = builder.Build();

            return Task.CompletedTask;
        }
    }
}