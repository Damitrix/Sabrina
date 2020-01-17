using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Sabrina.Models;
using System;
using System.Threading.Tasks;

namespace Sabrina.Entities.SettingsModules
{
	internal class CBTModule : SettingsModule
	{
		public CBTModule(DiscordContext context, DiscordDmChannel dm, CommandContext ctx, long userId) : base(context, dm, ctx, userId)
		{
		}

		public override string FriendlyName { get; internal set; } = "CBT";
		internal override string[] _keys { get; set; } = new[] { "cbt", "cock", "and", "ball", "torture" };

		internal override async Task<bool> Run()
		{
			int? cbtLevel = null;

			var builder = new DiscordEmbedBuilder()
			{
				Title = "At which level do you want your cbt tasks to be? This is pretty much a difficulty setting.",
				Url = "http://IJustWantThisToBeBlue.com"
			};

			builder.AddField("If you don't like cbt, choose this.", UserSetting.CBTLevel.None.ToString());
			builder.AddField("If you're new to cbt, or just not interested that much. take this.", UserSetting.CBTLevel.Light.ToString());
			builder.AddField("This setting is for regular cbt", UserSetting.CBTLevel.Normal.ToString());
			builder.AddField("Devices like a ball crusher and cockrings are basically your life? Choose this.", UserSetting.CBTLevel.Hardcore.ToString());

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

			if (Enum.TryParse(m.Result.Content, true, out UserSetting.CBTLevel bondageLevelPreference))
			{
				cbtLevel = (int)bondageLevelPreference;
			}
			else
			{
				await _dm.SendMessageAsync($"Sorry, I didn't get that. You have to precisely enter the name of one of the levels.");
			}

			await UserSetting.SetSettingAsync(_userId, UserSetting.SettingID.CBTLevel, cbtLevel.ToString(), _context);

			return true;
		}
	}
}