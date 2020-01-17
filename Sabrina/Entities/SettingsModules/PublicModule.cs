using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Sabrina.Models;
using System;
using System.Threading.Tasks;

namespace Sabrina.Entities.SettingsModules
{
	internal class PublicModule : SettingsModule
	{
		public PublicModule(DiscordContext context, DiscordDmChannel dm, CommandContext ctx, long userId) : base(context, dm, ctx, userId)
		{
		}

		public override string FriendlyName { get; internal set; } = "Public";
		internal override string[] _keys { get; set; } = new[] { "public" };

		internal override async Task<bool> Run()
		{
			int? publicLevel = null;

			var builder = new DiscordEmbedBuilder()
			{
				Title = "At which level do you want your public tasks to be? This is pretty much a difficulty setting.",
				Url = "http://IJustWantThisToBeBlue.com"
			};

			builder.AddField("If you don't like public tasks, choose this.", UserSetting.PublicLevel.None.ToString());
			builder.AddField("If you're new to public tasks, or just not interested that much. take this.", UserSetting.PublicLevel.Light.ToString());
			builder.AddField("This setting is for regular public tasks", UserSetting.PublicLevel.Normal.ToString());
			builder.AddField("You like stopping at the nearest corner, to masturbate in the wild? Choose this.", UserSetting.PublicLevel.Hardcore.ToString());

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

			if (Enum.TryParse(m.Result.Content, true, out UserSetting.PublicLevel publicLevelPreference))
			{
				publicLevel = (int)publicLevelPreference;
			}
			else
			{
				await _dm.SendMessageAsync($"Sorry, I didn't get that. You have to precisely enter the name of one of the levels.");
			}

			await UserSetting.SetSettingAsync(_userId, UserSetting.SettingID.PublicLevel, publicLevel.ToString(), _context);

			return true;
		}
	}
}