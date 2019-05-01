// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NoOrgasm.cs" company="">
//
// </copyright>
// <summary>
//   Defines the NoOrgasm type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sabrina.Entities.WheelOutcomes
{
    using DSharpPlus.Entities;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using WheelOutcome = Persistent.WheelOutcome;

    /// <summary>
    /// The no orgasm Outcome.
    /// </summary>
    internal sealed class NoOrgasm : WheelOutcome
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoOrgasm"/> class.
        /// </summary>
        /// <param name="outcome">
        /// The outcome.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        public NoOrgasm(WheelExtension.Outcome outcome, Dictionary<UserSettingExtension.SettingID, UserSetting> settings, List<WheelUserItem> items, Dependencies dependencies) : base(outcome, settings, items, dependencies)
        {
        }

        /// <summary>
        /// Gets or sets the chance.
        /// </summary>
        public override int Chance { get; protected set; } = 40;

        /// <summary>
        /// Gets or sets the denial time.
        /// </summary>
        public override TimeSpan DenialTime { get; protected set; }

        /// <summary>
        /// Gets or sets the embed.
        /// </summary>
        public override DiscordEmbed Embed { get; protected set; }

        /// <summary>
        /// Gets or sets the outcome.
        /// </summary>
        public override WheelExtension.Outcome Outcome { get; protected set; }

        /// <summary>
        /// Gets or sets the text to display to the user.
        /// </summary>
        public override string Text { get; protected set; }

        /// <summary>
        /// Gets or sets the wheel locked time.
        /// </summary>
        public override TimeSpan WheelLockedTime { get; protected set; }

        public override Task BuildAsync()
        {
            if (!Outcome.HasFlag(WheelExtension.Outcome.Denial))
            {
                Outcome = WheelExtension.Outcome.NotSet;
                return Task.CompletedTask;
            }

            int minNum = 1;
            int maxNum = 4;

            WheelExtension.WheelDifficultyPreference difficulty = WheelExtension.WheelDifficultyPreference.Default;

            if (_settings.ContainsKey(UserSettingExtension.SettingID.WheelDifficulty))
            {
                difficulty = _settings.First(setting => setting.Key == UserSettingExtension.SettingID.WheelDifficulty).Value.GetValue<WheelExtension.WheelDifficultyPreference>();
            }

            maxNum *= (int)difficulty;

            int rndNumber = Helpers.RandomGenerator.RandomInt(minNum, maxNum);

            this.DenialTime = new TimeSpan(rndNumber, 0, 0);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = "No Orgasm for you!",
                Description = "Try again in a few hours :P"
            };

            this.Embed = builder.Build();
            this.Text = "No orgasm for you! Try again in a few hours :P";
            this.Outcome = WheelExtension.Outcome.Denial;

            return Task.CompletedTask;
        }
    }
}