using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Sabrina.Models;
using System;
using System.Threading.Tasks;

namespace Sabrina.Entities.SettingsModules
{
	public class WheelDifficultyModule : SettingsModule
	{
		public WheelDifficultyModule(DiscordContext context, DiscordDmChannel dm, CommandContext ctx, long userId) : base(context, dm, ctx, userId)
		{
		}

		public override string FriendlyName { get; internal set; } = "Wheel difficulty";
		internal override string[] _keys { get; set; } = new[] { "wheel", "difficulty" };

		internal override async Task<bool> Run()
		{
			int? wheelDifficulty = null;

			while (!wheelDifficulty.HasValue)
			{
				var builder = new DiscordEmbedBuilder()
				{
					Title = "How Difficult would you like the Wheel to be? Lower Difficulties will lower your required Edges and work, but also your Chance for a good ending :)",
					Url = "http://IJustWantThisToBeBlue.com"
				};

				builder.AddField("Easiest Setting. Almost no Edges, will leave you in ruins.", UserSetting.WheelDifficultyPreference.Baby.ToString());
				builder.AddField(
					"Easy Setting, for when you are just starting with Edging.",
					UserSetting.WheelDifficultyPreference.Easy.ToString());
				builder.AddField(
					"Default. This is how the Wheel was before the Settings arrived, and how it is before you set up the settings.",
					UserSetting.WheelDifficultyPreference.Default.ToString());
				builder.AddField("Pretty Challenging.", UserSetting.WheelDifficultyPreference.Hard.ToString());
				builder.AddField("This will make every single roll Hardcore. High risk, High reward though.", UserSetting.WheelDifficultyPreference.Masterbater.ToString());

				await _dm.SendMessageAsync(embed: builder.Build());

				var m = await _ctx.Client.GetInteractivity().WaitForMessageAsync(
					x => x.Channel.Id == _dm.Id
						 && x.Author.Id == _ctx.Member.Id,
					TimeSpan.FromSeconds(240));

				if (m.TimedOut)
				{
					await _dm.SendMessageAsync($"Or just don't respond at all. That's ok too :(");
					return false;
				}

				if (Enum.TryParse(m.Result.Content, true, out UserSetting.WheelDifficultyPreference wheelDifficultyPreference))
				{
					wheelDifficulty = (int)wheelDifficultyPreference;
				}
				else
				{
					await _dm.SendMessageAsync($"Sorry, I didn't ge that. You have to precisely enter the name of one of the difficulties.");
				}
			}

			await UserSetting.SetSettingAsync(_userId, UserSetting.SettingID.WheelDifficulty, wheelDifficulty.ToString(), _context);

			return true;
		}
	}
}