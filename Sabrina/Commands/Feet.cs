// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Feet.cs" company="SalemsTools">
//     Do whatever
// </copyright>
// <summary>
// Defines the Feet type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Sabrina.Commands
{
	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using Sabrina.Bots;
	using Sabrina.Entities;
	using Sabrina.Models;
	using System.Threading;
	using System.Threading.Tasks;

	/// <summary>
	/// The feet Command Group.
	/// </summary>
	public class Feet : BaseCommandModule
	{
		private readonly SankakuBot _sankakuBot;
		private readonly WaifuJOIBot _waifujoiBot;

		public Feet(SankakuBot sBot, WaifuJOIBot wjBot)
		{
			_sankakuBot = sBot;
			_waifujoiBot = wjBot;
		}

		[Command("boostDL"), Hidden, Description("Posts some more Feet. Can be used 4 times a day")]
		public async Task BoostDeepLearningFeetPics(CommandContext ctx)
		{
			using var context = new DiscordContext();

			var minTime = DateTime.Now - TimeSpan.FromHours(6);
			var boosts = context.Boost.Where(b => b.Date > minTime && (b.Channel == null || b.Channel == Convert.ToInt64(ctx.Channel.Id)));

			if (await boosts.CountAsync() > 4)
			{
				await ctx.RespondAsync($"You have to wait before boosting again. Next one is available in {(TimeSpan.FromHours(6) - (DateTime.Now - boosts.Last().Date)).TotalMinutes} minutes.");
				return;
			}

			var sabrinaSettings = await context.SabrinaSettings.FindAsync(Convert.ToInt64(ctx.Guild.Id));

			if (sabrinaSettings.FeetChannel == null)
			{
				sabrinaSettings.FeetChannel = Convert.ToInt64(ctx.Channel.Id);
			}

			var channel = await ctx.Client.GetChannelAsync(Convert.ToUInt64(sabrinaSettings.FeetChannel));

			if (sabrinaSettings.FeetChannel.Value != Convert.ToInt64(ctx.Channel.Id))
			{
				await ctx.RespondAsync($"You cannot issue this command from this Channel. Please use {channel.Mention}");
				return;
			}

			var picsToPost = Helpers.RandomGenerator.RandomInt(2, 8);
			Parallel.For(0, picsToPost, i =>
			 {
				 try
				 {
					 _ = _sankakuBot.PostRandom(channel, i);
				 }
				 catch (Exception e)
				 {
					 Console.WriteLine(e);
				 }
				 //await SankakuBot.PostPrediction(Convert.ToInt64(ctx.User.Id) ,channel);
			 });
		}

		/// <summary>
		/// The boost feet pics Command.
		/// </summary>
		/// <param name="ctx">The Command Context.</param>
		/// <returns>A <see cref="Task"/>.</returns>
		[Command("boost"), Description("Posts some more Feet. Can be used 4 times a day")]
		public async Task BoostFeetPics(CommandContext ctx)
		{
			using var context = new DiscordContext();

			var minTime = DateTime.Now - TimeSpan.FromHours(6);
			var boosts = context.Boost.Where(b => b.Date > minTime && b.Channel == Convert.ToInt64(ctx.Channel.Id));

			if (await boosts.CountAsync() > 4)
			{
				await ctx.RespondAsync($"You have to wait before boosting again. Next one is available in {(TimeSpan.FromHours(6) - (DateTime.Now - boosts.Last().Date)).TotalMinutes} minutes.");
				return;
			}

			var sabrinaSettings = await context.SabrinaSettings.FindAsync(Convert.ToInt64(ctx.Guild.Id));

			if (sabrinaSettings.FeetChannel == null)
			{
				sabrinaSettings.FeetChannel = Convert.ToInt64(ctx.Channel.Id);
			}

			var channel = await ctx.Client.GetChannelAsync(Convert.ToUInt64(sabrinaSettings.FeetChannel));

			if (sabrinaSettings.FeetChannel.Value != Convert.ToInt64(ctx.Channel.Id))
			{
				await ctx.RespondAsync($"You cannot issue this command from this Channel. Please use {channel.Mention}");
				return;
			}

			var picsToPost = Helpers.RandomGenerator.RandomInt(2, 5);

			new Thread(async () =>
			{
				for (int i = 0; i < picsToPost; i++)
				{
					if (i % 2 == 0)
					{
						await _sankakuBot.PostRandom(channel, i);
					}
					else
					{
						await _waifujoiBot.PostRandom(channel, new CancellationToken());
					}
				}
			}).Start();

			context.Boost.Add(new Boost()
			{
				Amount = picsToPost,
				Date = DateTime.Now,
				Channel = Convert.ToInt64(ctx.Channel.Id)
			});
			await context.SaveChangesAsync();
		}
	}
}