using DSharpPlus.Entities;
using Sabrina.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sabrina.Models.UserSetting;

namespace Sabrina.Entities.PunishmentModules
{
	internal class SissyModule : PunishmentModule
	{
		public SissyModule(Dictionary<UserSetting.SettingID, UserSetting> settings, List<WheelUserItem> items) : base(settings, items)
		{
			SissyLevel sissyLevel = SissyLevel.None;
			if (Settings.ContainsKey(SettingID.SissyLevel))
			{
				sissyLevel = Settings[SettingID.SissyLevel].GetValue<SissyLevel>();
			}

			if (sissyLevel == SissyLevel.None)
			{
				Chance = 0;

				((List<UserSetting.SettingID>)RequiredSettings).Add(UserSetting.SettingID.SissyLevel);
			}
		}

		public override int Chance { get; internal set; } = 10;
		public override TimeSpan DenialTime { get; internal set; }
		public override DiscordEmbed Embed { get; internal set; }
		public override TimeSpan WheelLockTime { get; internal set; }

		public override Task Generate()
		{
			SissyLevel sissyLevel = SissyLevel.None;
			if (Settings.ContainsKey(SettingID.SissyLevel))
			{
				sissyLevel = Settings[SettingID.SissyLevel].GetValue<SissyLevel>();
			}

			PublicLevel publicLevel = PublicLevel.None;
			if (Settings.ContainsKey(SettingID.PublicLevel))
			{
				publicLevel = Settings[SettingID.PublicLevel].GetValue<PublicLevel>();
			}

			UserSetting.WheelDifficultyPreference difficulty = UserSetting.WheelDifficultyPreference.Default;
			if (Settings.ContainsKey(SettingID.WheelDifficulty))
			{
				difficulty = Settings[SettingID.WheelDifficulty].GetValue<UserSetting.WheelDifficultyPreference>();
			}

			List<WheelUserItem> clothing = Items.Where(item => WheelItemExtension.GetItemCategory((WheelItemExtension.Item)item.ItemId) == WheelItemExtension.Item.Clothing).ToList();

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

				postfix = i switch
				{
					2 => " and ",

					1 => ". ",

					_ => ", ",
				};
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