using Configuration;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sabrina.Commands
{
	[Group("tod"), Aliases("bottle"), Description("Group used for truth or dare"), Hidden]
	public class TruthOrDare : BaseCommandModule
	{
		private const long _RoleID = 628678175685214260;

		public TruthOrDare()
		{
		}

		[Command("next"), Aliases("spin")]
		[Description("Get next truth-or-dare player")]
		public async Task NextAsync(CommandContext ctx)
		{
			var guild = await ctx.Client.GetGuildAsync(ctx.Guild.Id);

			var allMembers = await guild.GetAllMembersAsync();

			var membersWithRole = allMembers.Where(m => m.Roles.Any(r => r.Id == _RoleID));

			var count = membersWithRole.Count();
			if (count < 2)
			{
				await ctx.RespondAsync($"Not enough players.");
				return;
			}

			var rnd = Entities.Helpers.RandomGenerator.RandomInt(0, count);

			var nextMember = membersWithRole.ElementAt(rnd);

			await ctx.RespondAsync($"{nextMember.Mention}, truth or dare? :3");
		}

		[Command("setup"), Aliases("start")]
		[Description("Setup a new truth or dare")]
		public async Task SetupAsync(CommandContext ctx, [Description("Time in minutes after whic to start the next round")] int startIn)
		{
			if (startIn > 30)
			{
				await ctx.RespondAsync("Cannot wait longer than 30 minutes");
				return;
			}

			var guild = await ctx.Client.GetGuildAsync(ctx.Guild.Id);

			var allMembers = await guild.GetAllMembersAsync();

			var membersWithRole = allMembers.Where(m => m.Roles.Any(r => r.Id == _RoleID));

			foreach (var member in membersWithRole)
			{
				await member.RevokeRoleAsync(guild.GetRole(_RoleID), "New truth or dare");
			}

			DateTime nextRoundStart = DateTime.Now + TimeSpan.FromMinutes(startIn);

			var emoji = DiscordEmoji.FromName(ctx.Client, Config.Emojis.Confirms.First());

			var messageText = $"React with {emoji} to join the truth or dare! The next one will start in {(nextRoundStart - DateTime.Now).ToString(@"hh\:mm\:ss")}";

			var announcementMessage = await ctx.Channel.SendMessageAsync(messageText);

			await announcementMessage.CreateReactionAsync(emoji);

			while (DateTime.Now < nextRoundStart)
			{
				messageText = $"React with {emoji} to join the truth or dare! The next one will start in {(nextRoundStart - DateTime.Now).ToString(@"hh:\mm\:ss")}";
				await announcementMessage.ModifyAsync(messageText);
				await Task.Delay(30000);
			}

			var reactedMembers = (await announcementMessage.GetReactionsAsync(emoji)).Where(r => !r.IsBot).ToArray();

			await announcementMessage.DeleteAsync();

			if (reactedMembers.Count() == 0)
			{
				await ctx.RespondAsync($"Or just fucking don't");
				return;
			}

			if (reactedMembers.Count() == 1)
			{
				await ctx.RespondAsync($"Get a friend {reactedMembers.First().Username}");
				return;
			}

			foreach (var member in reactedMembers)
			{
				await guild.Members.First(m => m.Key == member.Id).Value.GrantRoleAsync(guild.GetRole(_RoleID), "Registered for truth-or-dare");
			}

			await ctx.RespondAsync($"Round start for: {String.Join(',', reactedMembers.Select(m => m.Username))}");
		}
	}
}