using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Sabrina.Models;
using System;
using System.Threading.Tasks;

namespace Sabrina.Entities.SettingsModules
{
	internal class AnalModule : SettingsModule
	{
		public AnalModule(DiscordContext context, DiscordDmChannel dm, CommandContext ctx, long userId) : base(context, dm, ctx, userId)
		{
		}

		public override string FriendlyName { get; internal set; } = "Anal";
		internal override string[] _keys { get; set; } = new[] { "anal" };

		internal override async Task<bool> Run()
		{
			int? analLevel = null;

			var builder = new DiscordEmbedBuilder()
			{
				Title = "At which level do you want your anal tasks to be? This is pretty much a difficulty setting.",
				Url = "http://IJustWantThisToBeBlue.com"
			};

			builder.AddField("If you don't like anal, choose this.", UserSetting.AnalLevel.None.ToString());
			builder.AddField("If you're new to anal, or just not interested that much. take this.", UserSetting.AnalLevel.Light.ToString());
			builder.AddField("This setting is for regular anal", UserSetting.AnalLevel.Normal.ToString());
			builder.AddField("You've got a dildo up your ass even now? Choose this.", UserSetting.AnalLevel.Hardcore.ToString());

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

			if (Enum.TryParse(m.Result.Content, true, out UserSetting.AnalLevel analLevelPreference))
			{
				analLevel = (int)analLevelPreference;
			}
			else
			{
				await _dm.SendMessageAsync($"Sorry, I didn't get that. You have to precisely enter the name of one of the levels.");
			}

			await UserSetting.SetSettingAsync(_userId, UserSetting.SettingID.AnalLevel, analLevel.ToString(), _context);

			return true;
		}
	}
}