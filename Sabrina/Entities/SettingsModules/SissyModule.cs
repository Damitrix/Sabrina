using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Sabrina.Models;
using System;
using System.Threading.Tasks;

namespace Sabrina.Entities.SettingsModules
{
	internal class SissyModule : SettingsModule
	{
		public SissyModule(DiscordContext context, DiscordDmChannel dm, CommandContext ctx, long userId) : base(context, dm, ctx, userId)
		{
		}

		public override string FriendlyName { get; internal set; } = "sissy/trap";
		internal override string[] _keys { get; set; } = new[] { "sissy", "trap" };

		internal override async Task<bool> Run()
		{
			int? sissyLevel = null;

			var builder = new DiscordEmbedBuilder()
			{
				Title = "At which level do you want your sissy/trap tasks to be? This is pretty much a difficulty setting.",
				Url = "http://IJustWantThisToBeBlue.com"
			};

			builder.AddField("If you don't like being a sissy and traps in general, choose this.", UserSetting.SissyLevel.None.ToString());
			builder.AddField("If you're new to being a sissy and traps, or just not interested that much. take this.", UserSetting.SissyLevel.Light.ToString());
			builder.AddField("This setting is for regular sissy tasks and general trap porn.", UserSetting.SissyLevel.Normal.ToString());
			builder.AddField("You're dressed with a cute skirt and socks right now? Maybe *only* a skirt and socks? :smug:", UserSetting.SissyLevel.Hardcore.ToString());

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

			if (Enum.TryParse(m.Result.Content, true, out UserSetting.SissyLevel sissyLevelPreference))
			{
				sissyLevel = (int)sissyLevelPreference;
			}
			else
			{
				await _dm.SendMessageAsync($"Sorry, I didn't get that. You have to precisely enter the name of one of the levels.");
			}

			await UserSetting.SetSettingAsync(_userId, UserSetting.SettingID.SissyLevel, sissyLevel.ToString(), _context);

			return true;
		}
	}
}