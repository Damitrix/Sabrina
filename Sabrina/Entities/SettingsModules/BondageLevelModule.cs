using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Sabrina.Models;
using System;
using System.Threading.Tasks;

namespace Sabrina.Entities.SettingsModules
{
	internal class BondageLevelModule : SettingsModule
	{
		public BondageLevelModule(DiscordContext context, DiscordDmChannel dm, CommandContext ctx, long userId) : base(context, dm, ctx, userId)
		{
		}

		public override string FriendlyName { get; internal set; } = "bondage level";
		internal override string[] _keys { get; set; } = new[] { "bondage", "level", "difficulty", "preference" };

		internal override async Task<bool> Run()
		{
			int? bondageLevel = null;

			var builder = new DiscordEmbedBuilder()
			{
				Title = "At which level do you want your bondage tasks to be? This is like a difficulty setting.",
				Url = "http://IJustWantThisToBeBlue.com"
			};

			builder.AddField("If you don't like bondage, choose this.", UserSetting.BondageLevel.None.ToString());
			builder.AddField("If you're new to bondage, or just not interested that much. take this.", UserSetting.BondageLevel.Light.ToString());
			builder.AddField("This setting is for regular bondage play", UserSetting.BondageLevel.Normal.ToString());
			builder.AddField("You like to rob on the floor, arms bound to your legs? Choose this.", UserSetting.BondageLevel.Hardcore.ToString());

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

			if (Enum.TryParse(m.Result.Content, true, out UserSetting.BondageLevel bondageLevelPreference))
			{
				bondageLevel = (int)bondageLevelPreference;
			}
			else
			{
				await _dm.SendMessageAsync($"Sorry, I didn't get that. You have to precisely enter the name of one of the levels.");
			}

			await UserSetting.SetSettingAsync(_userId, UserSetting.SettingID.BondageLevel, bondageLevel.ToString(), _context);

			await _dm.TriggerTypingAsync();
			await Task.Delay(500);
			await _dm.SendMessageAsync($"Do you want to set up your gear as well? If you don't do it, i won't know your items. (Y/N)");

			var addGearResponse = await _ctx.Client.GetInteractivity().WaitForMessageAsync(
				x => x.Channel.Id == _dm.Id
					 && x.Author.Id == _ctx.Member.Id,
				TimeSpan.FromSeconds(240));

			if (m.TimedOut)
			{
				await _dm.SendMessageAsync($"Or just don't respond at all. That's ok too :(");
				return false;
			}

			if (Helpers.RegexHelper.YesRegex.Match(addGearResponse.Result.Content).Success)
			{
				bool setupGear = await new BondageGearModule(_context, _dm, _ctx, _userId).Run();

				if (!setupGear)
				{
					return false;
				}
			}
			else
			{
				await _dm.TriggerTypingAsync();
				await Task.Delay(500);
				await _dm.SendMessageAsync($"That's ok, you can do it later :)");
			}

			return true;
		}
	}
}