using DSharpPlus.Entities;
using Sabrina.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WheelOutcome = Sabrina.Entities.Persistent.WheelOutcome;

namespace Sabrina.Entities.WheelOutcomes
{
    internal sealed class Bondage : WheelOutcome
    {
        public Bondage(WheelExtension.Outcome outcome,
                       Dictionary<UserSettingExtension.SettingID, UserSetting> settings,
                       List<WheelUserItem> items,
                       Dependencies dependencies)
            : base(outcome, settings, items, dependencies)
        {
        }

        public override int Chance { get; protected set; } = 10;
        public override TimeSpan DenialTime { get; protected set; }
        public override DiscordEmbed Embed { get; protected set; }
        public override WheelExtension.Outcome Outcome { get; protected set; }
        public override string Text { get; protected set; }
        public override TimeSpan WheelLockedTime { get; protected set; }

        public override Task BuildAsync()
        {
            if (!Outcome.HasFlag(WheelExtension.Outcome.Denial) && !Outcome.HasFlag(WheelExtension.Outcome.Task))
            {
                Outcome = WheelExtension.Outcome.NotSet;
                return Task.CompletedTask;
            }

            var difficulty = WheelExtension.WheelDifficultyPreference.Default;
            if (_settings.ContainsKey(UserSettingExtension.SettingID.WheelDifficulty))
            {
                difficulty = _settings.First(setting => setting.Key == UserSettingExtension.SettingID.WheelDifficulty).Value.GetValue<WheelExtension.WheelDifficultyPreference>();
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = "Get dressed!",
                Description = "Oops! Something went wrong while creating your Punishment :/"
            };

            UserSettingExtension.BondageLevel? bondageLevel = null;

            if (_settings.ContainsKey(UserSettingExtension.SettingID.BondageLevel))
            {
                bondageLevel = _settings.First(setting => setting.Key == UserSettingExtension.SettingID.BondageLevel).Value.GetValue<UserSettingExtension.BondageLevel>();
            }

            if (bondageLevel != null && bondageLevel == UserSettingExtension.BondageLevel.None)
            {
                Outcome = WheelExtension.Outcome.Denial;

                difficulty = (WheelExtension.WheelDifficultyPreference)((int)difficulty == 0 ? 1 : (int)difficulty);

                DenialTime = TimeSpan.FromHours((int)difficulty / 2f);

                builder.Description = "What? You don't wanna do Bondage? But i planned everything... Then I guess you won't be getting any fun now. Yes, that means you are denied!";

                Embed = builder.Build();

                return Task.CompletedTask;
            }
            else if (bondageLevel == null)
            {
                Outcome = WheelExtension.Outcome.Denial;
                DenialTime = TimeSpan.FromMinutes(10);

                builder.Description = "So... you've never told me if you like Bondage.... How about you do that right now? Use ``//settings setup``. You can re-roll in 10 minutes.";

                Embed = builder.Build();

                return Task.CompletedTask;
            }

            this.Chance *= (int)bondageLevel.Value * 2; // User likes Bondage, increase chance

            var bondageGear = _items.Where(item => WheelItemExtension.GetItemCategory((WheelItemExtension.Item)item.ItemId) == WheelItemExtension.Item.Bondage).ToList();

            if (!bondageGear.Any())
            {
                Outcome = WheelExtension.Outcome.Denial;

                builder.Description = "Hmm, you wanna do bondage, but haven't got any gear?";

                if (difficulty < WheelExtension.WheelDifficultyPreference.Default || bondageLevel == UserSettingExtension.BondageLevel.Light)
                {
                    WheelLockedTime = TimeSpan.FromMinutes(Helpers.RandomGenerator.RandomInt(2, 10));
                    builder.Description += " Well, i do like your enthusiasm though. You can spin my wheel again in a few minutes.";
                }
                else
                {
                    DenialTime = TimeSpan.FromMinutes((int)difficulty * Helpers.RandomGenerator.RandomInt(10, 30));
                    builder.Description += " How can you be so sparse-minded? This means denial for you.";
                }

                builder.Description += " For next time, fetch some cables as rope, or use socks as a gag. Add it to your collection with ``//settings setup``.";

                Embed = builder.Build();

                return Task.CompletedTask;
            }

            var gearNumber = Helpers.RandomGenerator.RandomInt(0, bondageGear.Count);
            var gear = bondageGear[gearNumber];

            switch (WheelItemExtension.GetItemSubCategory((WheelItemExtension.Item)gear.ItemId))
            {
                case WheelItemExtension.Item.ChastityDevice:
                    builder.Description = "After you're done with your session, put on your chastity device. Keep it on until your next session. (You can obviously take it off, if you can't pee with it or whatever)";
                    break;

                case WheelItemExtension.Item.Cuffs:
                    builder.Description = "I bet you look cute with those cuffs of your's put to use... Put them on now!" + Environment.NewLine
                                            + "You can remove them when the session is over.";
                    break;

                case WheelItemExtension.Item.Gag:
                    builder.Description = "I don't like a loose mouth. Get your gag ready and put it in." + Environment.NewLine
                                            + "You can take it out when the session is over.";
                    break;

                case WheelItemExtension.Item.Blindfold:
                    builder.Description = "Put on your Blindfold." + Environment.NewLine
                                            + "You can take it out when the session is over, or to read the Instructions.";
                    break;

                case WheelItemExtension.Item.String:
                    builder.Description = "Use a String, to bind your balls." + Environment.NewLine
                                            + "You can take it off when the session is over.";
                    break;

                case WheelItemExtension.Item.Rope:
                    string[] ropeUses = new[] { "legs", "arms", "arms, behind your back", "hands", "hands, behind your back" };

                    builder.Description = ":3" + Environment.NewLine
                                                + Environment.NewLine
                                                + Environment.NewLine
                                            + "...that means you're a bunny now. My rope bunny :3" + Environment.NewLine
                                            + $"Bind your {ropeUses[Helpers.RandomGenerator.RandomInt(0, ropeUses.Length)]} together." + Environment.NewLine
                                            + "It's up to you, to find out, how to spin my wheel after that :3" + Environment.NewLine
                                            + "You can un-bind yourself when the session is over.";
                    break;

                case WheelItemExtension.Item.Clamps:
                    int clampCount = (int)difficulty > (int)WheelExtension.WheelDifficultyPreference.Default ? 2 : 1;

                    string part = Helpers.RandomGenerator.RandomInt(0, 2) == 0 ? "balls" : "nipples";

                    builder.Description = $"Get {clampCount} of your nipple clamp{(clampCount == 2 ? "" : "s")}. Put {(clampCount == 2 ? "them" : "it")} on your {part}." + Environment.NewLine
                                        + $"You can remove {(clampCount == 2 ? "them" : "it")} when the session is over.";

                    break;

                default:
                    builder.Description = "You get a freebie, because I don't have any bondage options for your toys." + Environment.NewLine
                                            + "Enjoy it.";
                    break;
            }

            Embed = builder.Build();

            return Task.CompletedTask;
        }
    }
}