// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Ban.cs" company="">
// </copyright>
// <summary>
// Defines the Ban type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sabrina.Entities.WheelOutcomes
{
	using DSharpPlus.Entities;
	using Sabrina.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using WheelOutcome = Persistent.WheelOutcome;

	internal sealed class Ban : WheelOutcome
	{
		private static readonly string[] bans = new[] { "all porn besides anime/hentai", "all porn besides foot-related porn" };
		private readonly int maxBanTime = 7 + 1;
		private readonly int maxEdgeAmount = 15 + 1;
		private readonly int minBanTime = 2;
		private readonly int minEdgeAmount = 7;

		/// <summary>
		/// Initializes a new instance of the <see cref="Ban"/> class.
		/// </summary>
		/// <param name="outcome">The outcome.</param>
		/// <param name="_settings">The _settings.</param>
		public Ban(UserSetting.Outcome outcome, Dictionary<UserSetting.SettingID, UserSetting> settings, List<WheelUserItem> items, IServiceProvider services) : base(outcome, settings, items, services)
		{
		}

		private Ban()
		{
		}

		public override int Chance { get; protected set; } = 10;
		public override TimeSpan DenialTime { get; protected set; }
		public override DiscordEmbed Embed { get; protected set; }
		public override UserSetting.Outcome Outcome { get; protected set; }
		public override string Text { get; protected set; }
		public override TimeSpan WheelLockedTime { get; protected set; }

		public override Task BuildAsync()
		{
			if (!Outcome.HasFlag(UserSetting.Outcome.Task))
			{
				Outcome = UserSetting.Outcome.NotSet;
				return Task.CompletedTask;
			}

			if (_settings.Any(setting => setting.Key == UserSetting.SettingID.WheelTaskPreference))
			{
				var preference = (UserSetting.WheelTaskPreferenceSetting)int.Parse(_settings.First(setting => setting.Key == UserSetting.SettingID.WheelTaskPreference).Value.Value);

				if (preference.HasFlag(UserSetting.WheelTaskPreferenceSetting.Task))
				{
					this.Chance *= 6;
				}
			}

			DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

			int time = Helpers.RandomGenerator.RandomInt(this.minBanTime, this.maxBanTime);

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

				UserSetting.WheelDifficultyPreference difficulty = UserSetting.WheelDifficultyPreference.Default;

				if (_settings.ContainsKey(UserSetting.SettingID.WheelDifficulty))
				{
					difficulty = _settings.First(setting => setting.Key == UserSetting.SettingID.WheelDifficulty).Value.GetValue<UserSetting.WheelDifficultyPreference>();
				}

				switch (difficulty)
				{
					case UserSetting.WheelDifficultyPreference.Baby:
						edgeAmount /= 4;
						break;

					case UserSetting.WheelDifficultyPreference.Easy:
						edgeAmount /= 2;
						break;

					case UserSetting.WheelDifficultyPreference.Hard:
						edgeAmount *= 2;
						break;

					case UserSetting.WheelDifficultyPreference.Masterbater:
						edgeAmount *= 4;
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
			this.Outcome = UserSetting.Outcome.Task;

			return Task.CompletedTask;
		}
	}
}