using DSharpPlus.Entities;
using Sabrina.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sabrina.Models.UserSetting;

namespace Sabrina.Entities.PunishmentModules
{
    internal class CBTModule : PunishmentModule
    {
        public CBTModule(Dictionary<SettingID, UserSetting> settings, List<WheelUserItem> items) : base(settings, items)
        {
            CBTLevel cbtLevel = CBTLevel.None;
            if (Settings.ContainsKey(SettingID.CBTLevel))
            {
                cbtLevel = Settings[SettingID.CBTLevel].GetValue<CBTLevel>();
            }

            BondageLevel bondageLevel = BondageLevel.None;
            if (Settings.ContainsKey(SettingID.BondageLevel))
            {
                bondageLevel = Settings[SettingID.BondageLevel].GetValue<BondageLevel>();
            }

            if (cbtLevel == CBTLevel.None || bondageLevel == BondageLevel.None)
            {
                Chance = 0;

                if (!Settings.ContainsKey(SettingID.CBTLevel))
                {
                    ((List<UserSetting.SettingID>)RequiredSettings).Add(UserSetting.SettingID.CBTLevel);
                }

                if (!Settings.ContainsKey(SettingID.BondageLevel))
                {
                    ((List<UserSetting.SettingID>)RequiredSettings).Add(UserSetting.SettingID.BondageLevel);
                }
            }
        }

        public override int Chance { get; internal set; } = 50;
        public override TimeSpan DenialTime { get; internal set; }
        public override DiscordEmbed Embed { get; internal set; }
        public override TimeSpan WheelLockTime { get; internal set; }

        public override Task Generate()
        {
            CBTLevel cbtLevel = CBTLevel.None;
            if (Settings.ContainsKey(SettingID.CBTLevel))
            {
                cbtLevel = Settings[SettingID.CBTLevel].GetValue<CBTLevel>();
            }

            UserSetting.WheelDifficultyPreference difficulty = UserSetting.WheelDifficultyPreference.Default;
            if (Settings.ContainsKey(SettingID.WheelDifficulty))
            {
                difficulty = Settings[SettingID.WheelDifficulty].GetValue<UserSetting.WheelDifficultyPreference>();
            }

            BondageLevel bondageLevel = BondageLevel.None;
            if (Settings.ContainsKey(SettingID.BondageLevel))
            {
                bondageLevel = Settings[SettingID.BondageLevel].GetValue<BondageLevel>();
            }

            List<WheelUserItem> gear = Items.Where(item => WheelItemExtension.GetItemCategory((WheelItemExtension.Item)item.ItemId) == WheelItemExtension.Item.Bondage).ToList();

            const int bondageGearChance = 10;
            var bondageChancesSum = gear.Count * bondageGearChance;

            const int vanillaCbtChance = 30;

            var chanceSum = bondageChancesSum + vanillaCbtChance;

            var chanceValue = Helpers.RandomGenerator.RandomInt(0, chanceSum + 1);

            if (chanceValue < bondageChancesSum)
            {
                int gearIndex = -1;
                int cChance = 0;

                int i = 0;
                while (gearIndex == -1)
                {
                    cChance += bondageGearChance;
                    if (cChance > chanceValue)
                    {
                        gearIndex = i;
                    }

                    if (gearIndex == -1)
                    {
                        i++;
                    }
                }

                var calcGear = gear[i];

                Embed = GenerateBondagePunishment((WheelItemExtension.Item)calcGear.ItemId, bondageLevel, cbtLevel, difficulty);
            }
            else if (chanceValue < bondageChancesSum + vanillaCbtChance)
            {
                var vanillaChanceValue = Helpers.RandomGenerator.RandomInt(0, 4);

                string bodypart = vanillaChanceValue < 2 ? "dick" : "balls";
                string cbtType = vanillaChanceValue % 2 == 0 ? "flick" : "slap";

                int cbtAmount = GetPunishmentValue(cbtLevel, difficulty);

                var builder = new DiscordEmbedBuilder
                {
                    Title = "Hit it! (literally)",
                    Description = $"I want you, to {cbtType} your {bodypart} {cbtAmount} times"
                };
                Embed = builder.Build();

                WheelLockTime = TimeSpan.FromSeconds(cbtAmount);
            }

            return Task.CompletedTask;
        }

        private static int GetPunishmentValue(CBTLevel level, UserSetting.WheelDifficultyPreference difficulty)
        {
            int third = ((int)level * (int)difficulty * 3) / 3;
            return Helpers.RandomGenerator.RandomInt(third * 2, third * 4);
        }

        private DiscordEmbed GenerateBondagePunishment(WheelItemExtension.Item gear, BondageLevel bondageLevel, CBTLevel cbtLevel, UserSetting.WheelDifficultyPreference difficulty)
        {
            var builder = new DiscordEmbedBuilder
            {
                Title = "Get your toys ready!"
            };
            var punishValue = GetPunishmentValue(cbtLevel, difficulty) * (int)bondageLevel;
            WheelLockTime = TimeSpan.FromSeconds(punishValue);

            switch (WheelItemExtension.GetItemSubCategory(gear))
            {
                case WheelItemExtension.Item.ChastityDevice:
                    var chastityTime = TimeSpan.FromMinutes(punishValue);

                    builder.Description = $"You're going into the smallest chastity device you've got for the next {TimeResolver.TimeToString(chastityTime)}. Watch some porn and hump your pillow while doing so..";
                    DenialTime = chastityTime;
                    WheelLockTime = chastityTime;
                    break;

                case WheelItemExtension.Item.Rope:
                    builder.Description = $"Tie a knot with your {gear.ToFormattedText()} and hit your balls {punishValue} times with it..";
                    break;

                case WheelItemExtension.Item.Gag:
                    var gagTieTime = TimeSpan.FromMinutes(punishValue);

                    builder.Description = $"Tie your {gear.ToFormattedText()} to your cock for {TimeResolver.TimeToString(gagTieTime)}. Make the connection as short as you can..";

                    DenialTime = gagTieTime;
                    WheelLockTime = gagTieTime;
                    break;

                case WheelItemExtension.Item.Cuffs:
                    builder.Description = $"Cuff your hands behind your back with your {gear.ToFormattedText()} and tie them to your cock/balls. Then bow down {punishValue} times for me.";
                    break;

                case WheelItemExtension.Item.Clamps:
                    builder.Description = $"Take a shoe of yours and tie it to your Nipples with your {gear.ToFormattedText()}. Then jump {punishValue} times for me.";
                    break;

                case WheelItemExtension.Item.String:
                    builder.Description = $"Use your {gear.ToFormattedText()}, to tie your balls together. It should be sting a bit. Leave it for at least {punishValue} minutes like that.";
                    break;

                default:
                    builder.Description = $"You get a freebie, because I don't have any CBT options for your {gear.ToFormattedText()}. Enjoy it.";
                    break;
            }

            return builder.Build();
        }
    }
}