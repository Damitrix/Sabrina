using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Sabrina.Models;
using System;
using System.Threading.Tasks;

namespace Sabrina.Commands
{
	public class Edges : BaseCommandModule
	{
		private readonly DiscordContext _context;

		public Edges()
		{
			_context = new DiscordContext();
		}

		[Command("assignedges"), Hidden]
		[Description("Assign Edges to yourself")]
		public async Task AssignEdgesAsync(CommandContext ctx, int edges)
		{
			if (edges < 1)
			{
				await ctx.RespondAsync("You cannot assign yourself less than 1 Edge. For obvious reasons");
			}

			var user = await UserExtension.GetUser(Convert.ToInt64(Convert.ToInt64(ctx.Message.Author.Id)));

			user.WalletEdges += edges;
			await _context.SaveChangesAsync();

			await ctx.RespondAsync(
				$"I've assigned you {edges} edges. Your balance is now {user.WalletEdges} edges.");
		}

		[Command("assignedgesto"), Hidden]
		[Description("Assign Edges to someone")]
		[RequireRolesAttribute(RoleCheckMode.Any, "mistress", "aki's cutie")]
		public async Task AssignEdgesToAsync(CommandContext ctx, DiscordUser dcUser, int edges)
		{
			var user = await UserExtension.GetUser(Convert.ToInt64(Convert.ToInt64(ctx.Message.Author.Id)));

			user.WalletEdges += edges;

			await _context.SaveChangesAsync();

			await ctx.RespondAsync(
				$"I've assigned {dcUser.Username} {edges} edges. Their balance is now {user.WalletEdges} edges.");
		}

		[Command("edges"), Hidden]
		[Description("Show how much Edges you have left")]
		public async Task DisplayEdges(CommandContext ctx)
		{
			var user = await UserExtension.GetUser(Convert.ToInt64(Convert.ToInt64(ctx.Message.Author.Id)));

			await ctx.RespondAsync(
				$"Your Edge Balance is {user.WalletEdges} edges.");
		}

		[Command("edge"), Hidden]
		[Aliases("e")]
		[Description("Remove an Edge from your wallet")]
		[Cooldown(1, 20, CooldownBucketType.User)]
		public async Task HasEdgedAsync(CommandContext ctx)
		{
			var user = await UserExtension.GetUser(Convert.ToInt64(Convert.ToInt64(ctx.Message.Author.Id)));

			user.WalletEdges -= 1;
			user.TotalEdges += 1;

			await _context.SaveChangesAsync();

			await ctx.RespondAsync(
				$"You've told me you've edged 1 time. Your balance is now {user.WalletEdges} edges.");
		}
	}
}