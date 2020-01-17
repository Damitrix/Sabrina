using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Sabrina.Entities;
using Sabrina.Models;
using System;
using System.Threading.Tasks;

namespace Sabrina.Commands
{
	[Group("wheelsetting")]
	[Aliases("wheelsettings")]
	public class WheelSetting : BaseCommandModule
	{
		[Command("mode")]
		[Description("Set the mode of the orgasmwheel")]
		[RequireRolesAttribute(RoleCheckMode.Any, "mistress", "Mistress", "Master", "master", "Techno Kitty", "techno kitty")]
		public async Task SetModeAsync(
			CommandContext ctx,
			[Description("The name of the mode")]
			string mode)
		{
			if (Enum.TryParse(mode, true, out WheelSettingExtension.WheelMode wheelMode))
			{
				await WheelSettingExtension.SetMode(wheelMode, ctx.Guild.Id);

				await ctx.RespondAsync($"Mode for this guild is now \"{wheelMode.ToFormattedText()}\"!");
			}
			else
			{
				DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
				{
					Title = "Cannot parse",
					Description = $"I can't find a mode with the name \"{mode}\". Please use one of the following: "
				};

				foreach (WheelSettingExtension.WheelMode cMode in Enum.GetValues(typeof(WheelSettingExtension.WheelMode)))
				{
					builder.AddField(cMode.ToFormattedText(), ((int)cMode).ToString());
				}

				await ctx.RespondAsync(embed: builder.Build());
			}
		}
	}
}