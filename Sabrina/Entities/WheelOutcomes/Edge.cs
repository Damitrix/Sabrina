// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Edge.cs" company="SalemsTools">
//     Do whatever
// </copyright>
// <summary>
// Defines the Edge type.
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
	/// The edge Outcome.
	/// </summary>
	internal sealed class Edge : WheelOutcome
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Edge"/> class.
		/// </summary>
		/// <param name="outcome">The outcome.</param>
		/// <param name="settings">The settings.</param>
		public Edge(UserSetting.Outcome outcome, Dictionary<UserSetting.SettingID, UserSetting> settings, List<WheelUserItem> items, IServiceProvider services) : base(outcome, settings, items, services)
		{
		}

		private Edge()
		{
		}

		/// <summary>
		/// Gets or sets the chance of this being used.
		/// </summary>
		public override int Chance { get; protected set; } = 20;

		/// <summary>
		/// Gets or sets the denial time.
		/// </summary>
		public override TimeSpan DenialTime { get; protected set; }

		/// <summary>
		/// Gets or sets the embed to display to the user.
		/// </summary>
		public override DiscordEmbed Embed { get; protected set; }

		/// <summary>
		/// Gets or sets the outcome.
		/// </summary>
		public override UserSetting.Outcome Outcome { get; protected set; }

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
			if (!Outcome.HasFlag(UserSetting.Outcome.Edge))
			{
				Outcome = UserSetting.Outcome.NotSet;
				return Task.CompletedTask;
			}

			int edgeMinutes = Helpers.RandomGenerator.RandomInt(5, 31);

			UserSetting.WheelDifficultyPreference difficulty = UserSetting.WheelDifficultyPreference.Default;

			if (_settings.ContainsKey(UserSetting.SettingID.WheelDifficulty))
			{
				difficulty = _settings.First(setting => setting.Key == UserSetting.SettingID.WheelDifficulty).Value.GetValue<UserSetting.WheelDifficultyPreference>();
			}

			switch (difficulty)
			{
				case UserSetting.WheelDifficultyPreference.Baby:
					edgeMinutes = Convert.ToInt32((double)edgeMinutes / 2);
					break;

				case UserSetting.WheelDifficultyPreference.Easy:
					edgeMinutes = Convert.ToInt32((double)edgeMinutes / 1.5);
					break;

				case UserSetting.WheelDifficultyPreference.Hard:
					edgeMinutes = Convert.ToInt32((double)edgeMinutes * 1.5);
					break;

				case UserSetting.WheelDifficultyPreference.Masterbater:
					edgeMinutes = Convert.ToInt32((double)edgeMinutes * 2);
					break;
			}

			string flavorText = "Boring...";

			if (edgeMinutes > 10)
			{
				flavorText = "Too easy...";
			}
			else if (edgeMinutes > 15)
			{
				flavorText = "Not too bad!";
			}
			else if (edgeMinutes > 20)
			{
				flavorText = "Kind of difficult!";
			}
			else if (edgeMinutes > 25)
			{
				flavorText = "Ouch!";
			}

			DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
			{
				Title = flavorText,
				Footer = new DiscordEmbedBuilder.EmbedFooter()
				{
					Text = "And spin again afterwards. If you cum/ruin, use ``//came`` or ``//ruined``"
				}
			};

			UserSetting.WheelTaskPreferenceSetting? preference = null;

			if (_settings.ContainsKey(UserSetting.SettingID.WheelTaskPreference))
			{
				preference = _settings.First(setting => setting.Key == UserSetting.SettingID.WheelTaskPreference).Value.GetValue<UserSetting.WheelTaskPreferenceSetting>();
			}

			if (Helpers.RandomGenerator.RandomInt(0, 2) == 0)
			{
				if (preference != null && preference.Value.HasFlag(UserSetting.WheelTaskPreferenceSetting.Time))
				{
					this.Chance *= 3;
				}

				this.Text = $"{flavorText} Edge over and over (at least 30s Cooldown) for {edgeMinutes} minutes, then spin again~";

				builder.Description = $"Edge over and over (at least 30s Cooldown) for {edgeMinutes} minutes";

				this.WheelLockedTime = new TimeSpan(0, edgeMinutes, 0);
			}
			else
			{
				if (preference != null && preference.Value.HasFlag(UserSetting.WheelTaskPreferenceSetting.Amount))
				{
					this.Chance *= 3;
				}

				this.Text = $"{flavorText} Edge {edgeMinutes / 2} times, with 30 seconds of Cooldown in between. Then spin again~";

				builder.Description = $"Edge {edgeMinutes / 2} times, with 30 seconds of Cooldown in between.";

				this.WheelLockedTime = new TimeSpan(0, 0, (edgeMinutes / 2 * 28) + (edgeMinutes / 2 * 5));
			}

			this.Embed = builder.Build();
			this.Outcome = UserSetting.Outcome.Edge;

			return Task.CompletedTask;
		}
	}
}