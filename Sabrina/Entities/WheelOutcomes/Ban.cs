// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Ban.cs" company="">
//
// </copyright>
// <summary>
//   Defines the Ban type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sabrina.Entities.WheelOutcomes
{
    using DSharpPlus.Entities;
    using Sabrina.Entities.Persistent;
    using Sabrina.Models;
    using System;

    internal sealed class Ban : WheelOutcome
    {
        private static string[] bans = new[] { "all porn besides anime/hentai", "all porn besides foot-related porn" };
        private readonly int maxBanTime = 7 + 1;
        private readonly int maxEdgeAmount = 15 + 1;
        private readonly int minBanTime = 2;
        private readonly int minEdgeAmount = 7;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ban"/> class.
        /// </summary>
        /// <param name="outcome">
        /// The outcome.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        public Ban(WheelExtension.Outcome outcome, UserSettings settings, DiscordContext context) : base(outcome, settings, context)
        {
            if (settings.WheelTaskPreference == null)
            {
                settings.WheelTaskPreference = (int)WheelExtension.WheelTaskPreferenceSetting.Default;
                context.SaveChanges();
            }

            if (settings.WheelTaskPreference != null && ((WheelExtension.WheelTaskPreferenceSetting)settings.WheelTaskPreference).HasFlag(WheelExtension.WheelTaskPreferenceSetting.Task))
            {
                this.Chance *= 6;
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

            int time = Helpers.RandomGenerator.RandomInt(this.minBanTime, this.maxBanTime);
            settings.WheelDifficulty = settings.WheelDifficulty ?? 2;

            if (Helpers.RandomGenerator.RandomInt(0, 2) == 0)
            {
                builder.Title = "Content ban!";
                builder.Description =
                    $"You are banned from {bans[Helpers.RandomGenerator.RandomInt(0, bans.Length)]} for {time} days! If you already had the same ban, consider it reset.";
                builder.Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = "Now reroll!"
                };
            }
            else
            {
                int edgeAmount = Helpers.RandomGenerator.RandomInt(this.minEdgeAmount, this.maxEdgeAmount) * 2;

                switch ((WheelExtension.WheelDifficultyPreference)settings.WheelDifficulty)
                {
                    case WheelExtension.WheelDifficultyPreference.Baby:
                        edgeAmount = edgeAmount / 4;
                        break;

                    case WheelExtension.WheelDifficultyPreference.Easy:
                        edgeAmount = edgeAmount / 2;
                        break;

                    case WheelExtension.WheelDifficultyPreference.Hard:
                        edgeAmount = edgeAmount * 2;
                        break;

                    case WheelExtension.WheelDifficultyPreference.Masterbater:
                        edgeAmount = edgeAmount * 4;
                        break;
                }

                builder.Title = "Edging Task!";
                builder.Description =
                    $"You'll have to Edge {edgeAmount} times a Day, for {time} days! If you already had the same task, consider it reset.";
                builder.Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = "Now reroll!"
                };
            }

            this.Embed = builder.Build();
            this.Outcome = WheelExtension.Outcome.Task;
        }

        public override int Chance { get; protected set; } = 10;
        public override TimeSpan DenialTime { get; protected set; }
        public override DiscordEmbed Embed { get; protected set; }
        public override WheelExtension.Outcome Outcome { get; protected set; }
        public override string Text { get; protected set; }
        public override TimeSpan WheelLockedTime { get; protected set; }
    }
}