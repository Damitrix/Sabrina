// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WheelOutcome.cs" company="">
//
// </copyright>
// <summary>
//   Defines the WheelOutcome type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sabrina.Entities.Persistent
{
    using DSharpPlus.Entities;
    using Sabrina.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// The wheel outcome.
    /// </summary>
    internal abstract class WheelOutcome
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WheelOutcome"/> class.
        /// </summary>
        /// <param name="outcome">
        /// The outcome.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        protected WheelOutcome(WheelExtension.Outcome outcome, Dictionary<UserSettingExtension.SettingID, UserSetting> settings, List<WheelUserItem> items, Dependencies dependencies)
        {
            Outcome = outcome;
            _settings = settings;
            _dependencies = dependencies;
            _items = items;
        }

        /// <summary>
        /// Gets or sets the chance.
        /// </summary>
        public abstract int Chance { get; protected set; }

        /// <summary>
        /// Gets or sets the denial time.
        /// </summary>
        public abstract TimeSpan DenialTime { get; protected set; }

        /// <summary>
        /// Gets or sets the embed.
        /// </summary>
        public abstract DiscordEmbed Embed { get; protected set; }

        /// <summary>
        /// Gets or sets the outcome.
        /// </summary>
        public abstract WheelExtension.Outcome Outcome { get; protected set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public abstract string Text { get; protected set; }

        /// <summary>
        /// Gets or sets the wheel locked time.
        /// </summary>
        public abstract TimeSpan WheelLockedTime { get; protected set; }

        protected virtual Dependencies _dependencies { get; private set; }
        protected virtual List<WheelUserItem> _items { get; private set; }
        protected virtual Dictionary<UserSettingExtension.SettingID, UserSetting> _settings { get; private set; }

        public abstract Task BuildAsync();
    }
}