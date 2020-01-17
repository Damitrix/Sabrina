// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrgasmWheel.cs" company="SalemsTools">
//     Do whatever
// </copyright>
// <summary>
// Defines the OrgasmWheel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Configuration;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Sabrina.Entities;
using Sabrina.Entities.Persistent;
using Sabrina.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DbWheelOutcome = Sabrina.Models.WheelOutcome;
using WheelOutcome = Sabrina.Entities.Persistent.WheelOutcome;

namespace Sabrina.Commands
{
	/// <summary>
	/// The orgasm wheel Command Group.
	/// </summary>
	public class OrgasmWheel : BaseCommandModule
	{
		private readonly IServiceProvider _services;

		/// <summary>
		/// The wheel outcomes.
		/// </summary>
		private List<WheelOutcome> wheelOutcomes;

		public OrgasmWheel(IServiceProvider services)
		{
			_services = services;
		}

		/// <summary>
		/// The add link async.
		/// </summary>
		/// <param name="ctx">The Command Context.</param>
		/// <param name="creator">The creator.</param>
		/// <param name="type">The type.</param>
		/// <param name="url">The url.</param>
		/// <returns>The <see cref="Task"/>.</returns>
		[Command("addlink")]
		[Description("Add a Link to the OrgasmWheel")]
		[RequireRolesAttribute(RoleCheckMode.Any, "mistress", "techno kitty", "content creator", "trusted creator")]
		public async Task AddLinkAsync(
			CommandContext ctx,
			[Description("The Person who created the Content")]
			string creator,
			[Description("Picture or Video")] string type,
			[Description("The Link")] string url)
		{
			if (Enum.TryParse(type, true, out Link.ContentType linkType))
			{
				Link link = new Link
				{
					CreatorName = creator,
					Type = linkType,
					Url = url
				};
				link.Save();
				await ctx.RespondAsync("Link added!");
			}
			else
			{
				await ctx.RespondAsync(
					$"Cannot Parse \"{type}\". Please be sure to use either \"Video\", or \"Picture\".");
			}
		}

		[Command("came")]
		[Aliases("cum")]
		[Description("Use when you came")]
		public async Task Came(CommandContext ctx)
		{
			using var context = new DiscordContext();
			bool punish = true;
			var userId = Convert.ToInt64(ctx.Message.Author.Id);
			var user = await UserExtension.GetUser(userId, context);
			var difficulty = (await UserSetting.GetSettingAsync(userId, UserSetting.SettingID.WheelDifficulty, context)).GetValue<UserSetting.WheelDifficultyPreference>();
			var lastOutcome = await WheelOutcomeExtension.GetLastOutcome(userId, false, context);
			var lastallowedOrgasm = await WheelOutcomeExtension.GetLastOutcome(UserSetting.Outcome.Orgasm, userId, true, context);
			var lastUserOrgasm = await WheelOutcomeExtension.GetLastOutcome(UserSetting.Outcome.Orgasm, userId, true, context);

			if (lastOutcome == null)
			{
				await ctx.RespondAsync("Please start your *training* with ``//orgasmwheel`` first.");
				return;
			}

			DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

			var denialTime = TimeSpan.FromTicks(1);

			if (lastallowedOrgasm != null && lastUserOrgasm != null)
			{
				if (lastUserOrgasm.Time > lastallowedOrgasm.Time)
				{
					denialTime = TimeSpan.FromDays(Math.Round((int)difficulty + 1 * 1.5, 0, MidpointRounding.AwayFromZero));

					builder.Description = "What!? You already came after i allowed you to!!" + Environment.NewLine
											+ "Can you not even follow simple instructions?" + Environment.NewLine
											+ $"I'll give you {denialTime.Days} day{(denialTime.Days > 1 ? "s" : "")} of denial for that!";
				}
			}

			switch ((UserSetting.Outcome)lastOutcome.Type)
			{
				case UserSetting.Outcome.NotSet:
					builder.Description = "Yes, i hope you enjoyed that." + Environment.NewLine
											+ "I do reward my subs every now and then." + Environment.NewLine
											+ $"But don't get used to it.";
					break;

				case UserSetting.Outcome.Denial:
					denialTime = TimeSpan.FromDays(Math.Round((int)difficulty + 1 * 1.5, 0, MidpointRounding.AwayFromZero));

					builder.Description = "What!? I told you, that you'd be denied!" + Environment.NewLine
											+ "Can you not even follow simple instructions?" + Environment.NewLine
											+ $"I'll give you {denialTime.Days} day{(denialTime.Days > 1 ? "s" : "")} of denial for that!";
					break;

				case UserSetting.Outcome.Ruin:
					denialTime = TimeSpan.FromDays((int)difficulty + 1);

					builder.Description = "Oh, was my poor little boy not strong enough, to follow through with the ruin?" + Environment.NewLine
											+ "Guess you're gonna learn the hard way, to do what i say." + Environment.NewLine
											+ $"That means no cumming for {denialTime.Days} day{(denialTime.Days > 1 ? "s" : "")}. And no ruins of course!";
					break;

				case UserSetting.Outcome.Orgasm:
					punish = Helpers.RandomGenerator.RandomInt(0, 50) < (int)difficulty * 2;

					if (punish)
					{
						builder.Description = "That must've felt sooo good." + Environment.NewLine
											+ "But your luck ends here." + Environment.NewLine
											+ $"I don't really feel, like you should've come after all.";
					}
					else
					{
						builder.Description = "Yes, i hope you enjoyed that." + Environment.NewLine
											+ "I do reward my subs every now and then." + Environment.NewLine
											+ $"But don't get used to it.";
					}

					break;

				case UserSetting.Outcome.Edge:
					denialTime = TimeSpan.FromDays(Math.Round((int)difficulty + 1 * 1.5, 0, MidpointRounding.AwayFromZero));

					builder.Description = "So... you tipped over and didn't even ruin it? What a shame..." + Environment.NewLine
											+ "You're naturally going to have to make up for that." + Environment.NewLine
											+ $"I guess {denialTime.Days} day{(denialTime.Days > 1 ? "s" : "")} of denial should do for now.";
					break;

				case UserSetting.Outcome.Task:
					denialTime = TimeSpan.FromDays(Math.Round((int)difficulty + 1 * 1.5, 0, MidpointRounding.AwayFromZero));

					builder.Description = "So... you tipped over and didn't even ruin it? What a shame..." + Environment.NewLine
											+ "You're naturally going to have to make up for that." + Environment.NewLine
											+ $"I guess {denialTime.Days} day{(denialTime.Days > 1 ? "s" : "")} of denial should do for now.";
					break;
			}

			var prevDenialTime = user.DenialTime > DateTime.Now ? user.DenialTime : DateTime.Now;

			user.DenialTime = prevDenialTime + denialTime;

			var thisOutcome = new DbWheelOutcome()
			{
				IsUserReport = 1,
				Time = DateTime.Now,
				Type = (int)UserSetting.Outcome.Orgasm,
				UserId = userId
			};

			await context.WheelOutcome.AddAsync(thisOutcome);

			await context.SaveChangesAsync();

			if (punish)
			{
				builder.Description += Environment.NewLine + " And here's your punishment <3";
			}

			await ctx.RespondAsync(embed: builder.Build());

			if (punish)
			{
				await PunishmentInternal(ctx);
			}
		}

		/// <summary>
		/// The denial time Command.
		/// </summary>
		/// <param name="ctx">The Command Context.</param>
		/// <returns>The <see cref="Task"/>.</returns>
		[Command("denialtime")]
		[Description("Shows how much longer you should not come")]
		[Aliases("denieduntil")]
		public async Task DenialTimeAsync(CommandContext ctx)
		{
			using DiscordContext context = new DiscordContext();

			var userId = Convert.ToInt64(ctx.Message.Author.Id);
			Users user = await UserExtension.GetUser(ctx.Message.Author.Id);
			var difficulty = (await UserSetting.GetSettingAsync(userId, UserSetting.SettingID.WheelDifficulty, context)).GetValue<UserSetting.WheelDifficultyPreference>();
			var denialString = "I guess I could give you another chance to cum...";
			var wheelLockedString = "So how about you test your luck?";

			if (user.DenialTime != null && user.DenialTime > DateTime.Now)
			{
				TimeSpan timeLeft = user.DenialTime.Value - DateTime.Now;

				difficulty = difficulty == 0 ? UserSetting.WheelDifficultyPreference.Default : difficulty;

				var ballChance = 100 - (20 / (int)difficulty) - (timeLeft.TotalDays * 5);
				var generatedChance = Helpers.RandomGenerator.RandomInt(0, 101);

				if (generatedChance > ballChance)
				{
					denialString = "*shakes a magic ball*" + Environment.NewLine
									+ $"*the ball reads {Math.Round(timeLeft.TotalHours, 0)}*" + Environment.NewLine
									+ $"...I say no.";
				}
				else if (timeLeft > TimeSpan.FromHours(24))
				{
					denialString = "Haha, no, you won't cum today.";
				}
				else if (timeLeft > TimeSpan.FromHours(12))
				{
					denialString =
						"Ask me again after you've slept a bit... Or gone to work or whatever, I don't care.";
				}
				else if (timeLeft > TimeSpan.FromHours(6))
				{
					denialString = "Don't be ridiculous. You won't get a chance to cum now.";
				}
				else if (timeLeft > TimeSpan.FromHours(2))
				{
					denialString = "Maybe later. I don't feel like you should cum right now.";
				}
				else if (timeLeft > TimeSpan.FromMinutes(20))
				{
					denialString = "You won't cum right now. How about you try again in... say... 30 minutes? An hour?";
				}
				else
				{
					denialString = "No, you won't get a chance now. I want to see you squirm for just a few more minutes~";
				}

				if (user.LockTime != null && user.LockTime < DateTime.Now)
				{
					wheelLockedString = $"But i would allow you to spin right now, if you want {Environment.NewLine}" +
										$"*grins* {Environment.NewLine}" +
										"There\'s no way I\'ll let you cum though. You didn\'t deserve it yet.";
				}
				else
				{
					wheelLockedString = "";
				}
			}
			else
			{
				if (user.LockTime != null && user.LockTime > DateTime.Now)
				{
					TimeSpan lockTimeLeft = user.LockTime.Value - DateTime.Now;

					if (lockTimeLeft > TimeSpan.FromHours(24))
					{
						wheelLockedString = "But not today.";
					}
					else if (lockTimeLeft > TimeSpan.FromHours(12))
					{
						wheelLockedString = "But i don't want to, right now. Maybe later today.";
					}
					else if (lockTimeLeft > TimeSpan.FromHours(6))
					{
						wheelLockedString = "But I'm not in the mood right now. Ask me again in a few hours.";
					}
					else if (lockTimeLeft > TimeSpan.FromHours(2))
					{
						wheelLockedString = "But i'm kinda in the middle of something right now. We can play later.";
					}
					else
					{
						wheelLockedString = "I won't let you spin for the moment though :)";
					}
				}
			}

			await ctx.RespondAsync($"Hey {(await ctx.Client.GetUserAsync(Convert.ToUInt64(user.UserId))).Mention},\n" +
								   $"{denialString}\n" +
								   $"{wheelLockedString}");
		}

		/// <summary>
		/// The orgasm wheel Command.
		/// </summary>
		/// <param name="ctx">The Command Context.</param>
		/// <returns>The <see cref="Task"/>.</returns>
		[Command("fullorgasmwheel")]
		[Description("Spins the wheel until you get an outcome")]
		public async Task FullOrgasmWheel(CommandContext ctx)
		{
			await ctx.TriggerTypingAsync();

			using DiscordContext context = new DiscordContext();
			Users user = await UserExtension.GetUser(Convert.ToInt64(ctx.Message.Author.Id), context);

			if (!await DoInitialCheck(ctx))
			{
				return;
			}

			var wheelMode = await WheelSettingExtension.GetMode(ctx.Guild.Id, context);

			if (wheelMode == WheelSettingExtension.WheelMode.Infinite)
			{
				await ctx.RespondAsync("Wheel is set to infinite Mode. Please Change it to a Mode where an outcome is possible");
				return;
			}

			var isLocked = !await CheckLocked(ctx, user, context);

			if (isLocked)
			{
				return;
			}

			List<WheelOutcome> outcomes = new List<WheelOutcome>();

			var outcome = await GetOutcome(ctx, context);

			while (outcomes.All(o => o.Outcome != UserSetting.Outcome.Denial && o.Outcome != UserSetting.Outcome.Ruin && o.Outcome != UserSetting.Outcome.Orgasm))
			{
				outcome = await GetOutcome(ctx, context);
				outcome = await CheckDenial(ctx, user, outcome);
				outcomes.Add(await GetWheelOutcome(context, user, outcome));
			}

			//Last outcome will be end of wheel

			var outcomeTime = DateTime.Now;

			foreach (var wheelOutcome in outcomes)
			{
				outcomeTime += TimeSpan.FromSeconds(1);
				if (wheelOutcome.Embed != null)
				{
					await ctx.RespondAsync(embed: wheelOutcome.Embed);
				}
				else
				{
					await ctx.RespondAsync(wheelOutcome.Text);
				}

				DbWheelOutcome dbOutcome = new DbWheelOutcome()
				{
					IsUserReport = 0,
					Time = outcomeTime,
					Type = (int)outcome,
					UserId = user.UserId
				};

				context.WheelOutcome.Add(dbOutcome);

				if (wheelOutcome.GetType() == typeof(Entities.WheelOutcomes.Content))
				{
					((Entities.WheelOutcomes.Content)wheelOutcome).CleanUp(context);
				}
			}

			var endOutcome = outcomes.First(o => o.Outcome == UserSetting.Outcome.Denial || o.Outcome == UserSetting.Outcome.Ruin || o.Outcome == UserSetting.Outcome.Orgasm);

			if (endOutcome.WheelLockedTime != null && endOutcome.WheelLockedTime > TimeSpan.Zero)
			{
				switch (outcome)
				{
					case UserSetting.Outcome.Edge:
					case UserSetting.Outcome.Task:
						user.LockReason = ((int)UserSetting.LockReason.Task).ToString();
						break;

					default:
						user.LockReason = ((int)UserSetting.LockReason.Cooldown).ToString();
						break;
				}
			}

			if (user.DenialTime == null || user.DenialTime < DateTime.Now)
			{
				user.DenialTime = DateTime.Now;
			}

			if (user.LockTime == null || user.LockTime < DateTime.Now)
			{
				user.LockTime = DateTime.Now;
			}

			user.DenialTime += endOutcome.DenialTime;
			user.LockTime += endOutcome.WheelLockedTime;

			await context.SaveChangesAsync();

			await CheckUserHasOrgasm(ctx, outcome);
		}

		[Command("punishment")]
		[Aliases("punish")]
		[Description("Use when you want a punishment")]
		public async Task Punishment(CommandContext ctx)
		{
			try
			{
				var texts = new[] { "Here you go.", $"You just want a punishment? I like that {DiscordEmoji.FromName(ctx.Client, Config.Emojis.Smug)}", "You think you can earn a favor from me? ...maybe." };

				await ctx.RespondAsync(texts[Helpers.RandomGenerator.RandomInt(0, texts.Length)]).ConfigureAwait(false);
			}
			catch (Exception)
			{
				var texts = new[] { "Here you go.", $"You just want a punishment? I like that!", "You think you can earn a favor from me? ...maybe." };

				await ctx.RespondAsync(texts[Helpers.RandomGenerator.RandomInt(0, texts.Length)]).ConfigureAwait(false);
			}

			await PunishmentInternal(ctx);
		}

		/// <summary>
		/// The purge links command.
		/// </summary>
		/// <param name="ctx">The Command Context.</param>
		/// <returns>The <see cref="Task"/>.</returns>
		[Command("purgelinks")]
		[Description("Purges Links")]
		[RequireOwner]
		public async Task PurgeLinksAsync(CommandContext ctx)
		{
			List<Link> links = await Link.LoadAll();

			List<Link> linksToDelete = new List<Link>();

			foreach (Link origLink in links)
				foreach (Link currentLink in links)
					if (currentLink.FileName != origLink.FileName && currentLink.Url == origLink.Url)
					{
						linksToDelete.Add(currentLink);
					}

			var outString = string.Empty;

			foreach (Link link in linksToDelete)
			{
				outString += link.FileName + "\n";
				link.Delete();
			}

			await ctx.RespondAsync($"I've deleted the duplicates\n{outString}");
		}

		/// <summary>
		/// The remove profile async.
		/// </summary>
		/// <param name="ctx">The Command Context.</param>
		/// <param name="dcUser">The Discord user.</param>
		/// <returns>The <see cref="Task"/>.</returns>
		[Command("resetuser")]
		[Description("Reset a Users saved Data")]
		[Aliases("ru")]
		[RequireRolesAttribute(RoleCheckMode.Any, "mistress", "minion", "techno kitty", "Administrator", "Moderator")]
		public async Task ResetProfileAsync(CommandContext ctx,
			[Description("Mention the user here")] DiscordUser dcUser)
		{
			using DiscordContext context = new DiscordContext();
			Users user = await UserExtension.GetUser(Convert.ToInt64(dcUser.Id), context);

			user.DenialTime = DateTime.Now;
			user.BanTime = DateTime.Now;
			user.LockTime = DateTime.Now;
			user.SpecialTime = DateTime.Now;
			user.RuinTime = DateTime.Now;

			await context.SaveChangesAsync();

			await ctx.RespondAsync($"I've reset the Profile of {dcUser.Mention}.");
		}

		[Command("ruin")]
		[Aliases("ruined")]
		[Description("Use when you ruined")]
		public async Task Ruin(CommandContext ctx)
		{
			using var context = new DiscordContext();
			bool punish = true;
			var userId = Convert.ToInt64(ctx.Message.Author.Id);
			var user = await UserExtension.GetUser(userId, context);
			var difficulty = (await UserSetting.GetSettingAsync(userId, UserSetting.SettingID.WheelDifficulty, context)).GetValue<UserSetting.WheelDifficultyPreference>();
			var lastOutcome = await WheelOutcomeExtension.GetLastOutcome(userId, false, context);

			if (lastOutcome == null)
			{
				await ctx.RespondAsync("Please start your *session* with ``//orgasmwheel`` first.");

				return;
			}

			DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

			var denialTime = TimeSpan.FromTicks(1);

			switch ((UserSetting.Outcome)lastOutcome.Type)
			{
				case UserSetting.Outcome.NotSet:

					break;

				case UserSetting.Outcome.Denial:
					denialTime = TimeSpan.FromDays((int)difficulty + 1);

					builder.Description = "What!? I told you, that you'd be denied!" + Environment.NewLine
											+ "I mean... at least you tried. Still, you didn't do what i said, and this must be punished!" + Environment.NewLine
											+ $"I'll give you {denialTime.Days} day{(denialTime.Days > 1 ? "s" : "")} of denial for that!";
					break;

				case UserSetting.Outcome.Ruin:
					builder.Description = "Oh, this makes me so happy." + Environment.NewLine
											+ "I love seeing it just dribble out." + Environment.NewLine
											+ $"You're a good little sub.";
					punish = false;
					break;

				case UserSetting.Outcome.Orgasm:
					punish = Helpers.RandomGenerator.RandomInt(0, 50) < (int)difficulty * 2;

					if (punish)
					{
						builder.Description = "I can see, that my conditioning worked!" + Environment.NewLine
											+ "Even when you're allowed to cum, you still ruin it." + Environment.NewLine
											+ $"But... this wasn't a suggestion. It was an order.";
					}
					else
					{
						builder.Description = "I can see, that my conditioning worked!" + Environment.NewLine
											+ "Even when you're allowed to cum, you still ruin it." + Environment.NewLine
											+ $"Good boy.";
					}

					break;

				case UserSetting.Outcome.Edge:
					denialTime = TimeSpan.FromDays(Math.Round((int)difficulty + 1 * 1.5, 0, MidpointRounding.AwayFromZero));

					builder.Description = "So... you couldn't control it and just tipped over?" + Environment.NewLine
											+ "You're naturally going to have to make up for that." + Environment.NewLine
											+ $"I guess {denialTime.Days} day{(denialTime.Days > 1 ? "s" : "")} of denial should do it for now.";
					break;

				case UserSetting.Outcome.Task:
					denialTime = TimeSpan.FromDays(Math.Round((int)difficulty + 1 * 1.5, 0, MidpointRounding.AwayFromZero));

					builder.Description = "I'm not sure, how you can be so useless." + Environment.NewLine
											+ "You're naturally going to have to make up for that." + Environment.NewLine
											+ $"At least you tried... so I guess {denialTime.Days} day{(denialTime.Days > 1 ? "s" : "")} of denial should do it for now.";
					break;
			}

			var prevDenialTime = user.DenialTime > DateTime.Now ? user.DenialTime : DateTime.Now;

			user.DenialTime = prevDenialTime + denialTime;

			var thisOutcome = new DbWheelOutcome()
			{
				IsUserReport = 1,
				Time = DateTime.Now,
				Type = (int)UserSetting.Outcome.Orgasm,
				UserId = userId
			};

			await context.WheelOutcome.AddAsync(thisOutcome);

			await context.SaveChangesAsync();

			if (punish)
			{
				builder.Description += Environment.NewLine + "And here's your punishment <3";
			}

			await ctx.RespondAsync(embed: builder.Build());

			if (punish)
			{
				await PunishmentInternal(ctx);
			}
		}

		/// <summary>
		/// The show links Command.
		/// </summary>
		/// <param name="ctx">The Command Context.</param>
		/// <returns>The <see cref="Task"/>.</returns>
		[Command("showlinks")]
		[Description("Shows all Links")]
		[RequireOwner]
		public async Task ShowLinksAsync(CommandContext ctx)
		{
			List<Link> links = await Link.LoadAll();

			var text = "Here are all Links:\n```";

			foreach (Link link in links)
			{
				if ((text + link.Url).Length > 1999)
				{
					text += "```";
					await ctx.RespondAsync(text);
					text = "Here are more Links:\n```";
				}

				text += link.Url + "\n";
			}

			text += "```";

			await ctx.RespondAsync(text);
		}

		/// <summary>
		/// The orgasm wheel Command.
		/// </summary>
		/// <param name="ctx">The Command Context.</param>
		/// <returns>The <see cref="Task"/>.</returns>
		[Command("orgasmwheel")]
		[Description("Spins a wheel")]
		public async Task SpinNewWheelAsync(CommandContext ctx)
		{
			using DiscordContext context = new DiscordContext();
			Users user = await UserExtension.GetUser(Convert.ToInt64(ctx.Message.Author.Id), context);

			if (!await DoInitialCheck(ctx))
			{
				return;
			}

			var outcome = await GetOutcome(ctx, context);

			var isLocked = !await CheckLocked(ctx, user, context);

			if (isLocked)
			{
				return;
			}

			outcome = await CheckDenial(ctx, user, outcome);

			await ctx.TriggerTypingAsync();

			WheelOutcome wheelOutcome = await GetWheelOutcome(context, user, outcome);

			if (wheelOutcome.Embed != null)
			{
				await ctx.RespondAsync(embed: wheelOutcome.Embed);
			}
			else
			{
				await ctx.RespondAsync(wheelOutcome.Text);
			}

			DbWheelOutcome dbOutcome = new DbWheelOutcome()
			{
				IsUserReport = 0,
				Time = DateTime.Now,
				Type = (int)outcome,
				UserId = user.UserId
			};

			context.WheelOutcome.Add(dbOutcome);

			if (wheelOutcome.GetType() == typeof(Entities.WheelOutcomes.Content))
			{
				((Entities.WheelOutcomes.Content)wheelOutcome).CleanUp(context);
			}

			if (wheelOutcome.WheelLockedTime != null && wheelOutcome.WheelLockedTime > TimeSpan.Zero)
			{
				switch (outcome)
				{
					case UserSetting.Outcome.Edge:
					case UserSetting.Outcome.Task:
						user.LockReason = ((int)UserSetting.LockReason.Task).ToString();
						break;

					default:
						user.LockReason = ((int)UserSetting.LockReason.Cooldown).ToString();
						break;
				}
			}

			if (user.DenialTime == null || user.DenialTime < DateTime.Now)
			{
				user.DenialTime = DateTime.Now;
			}

			if (user.LockTime == null || user.LockTime < DateTime.Now)
			{
				user.LockTime = DateTime.Now;
			}

			user.DenialTime += wheelOutcome.DenialTime;
			user.LockTime += wheelOutcome.WheelLockedTime;

			await context.SaveChangesAsync();

			_ = CheckUserHasOrgasm(ctx, outcome);
		}

		/// <summary>
		/// The spin wheel Command.
		/// </summary>
		/// <param name="ctx">The Command Context.</param>
		/// <returns>A <see cref="Task"/>.</returns>
		[Command("oldorgasmwheel")]
		[Description("Spins the old wheel. Just in case the new one is broken.")]
		[Aliases("orgasmwheel1", "orgasmwheel2")]
		public async Task SpinWheelAsync(CommandContext ctx)
		{
			var outcome = Helpers.RandomGenerator.RandomInt(0, 100);

			string line;
			if (outcome < 92)
			{
				line = await LoadLineAsync($"{Config.BotFileFolders.WheelResponses}/Denial.txt");
			}
			else if (outcome < 96)
			{
				line = await LoadLineAsync($"{Config.BotFileFolders.WheelResponses}/Ruin.txt");
			}
			else
			{
				line = await LoadLineAsync($"{Config.BotFileFolders.WheelResponses}/Orgasm.txt");
			}

			await ctx.RespondAsync(line);
		}

		private async Task<float> CalculateCumScore(DiscordContext context, Users user)
		{
			float cumScore = 1;
			var lastOrgasm = await WheelOutcomeExtension.GetLastOutcome(UserSetting.Outcome.Orgasm, user.UserId, null, context);
			var lastRuin = await WheelOutcomeExtension.GetLastOutcome(UserSetting.Outcome.Ruin, user.UserId, null, context);

			if (lastOrgasm != null || lastRuin != null)
			{
				DbWheelOutcome lastCum = null;
				if (lastOrgasm != null && lastRuin != null)
				{
					lastCum = lastOrgasm?.Time > lastRuin?.Time ? lastOrgasm : lastRuin;
				}
				else
				{
					lastCum = lastOrgasm ?? lastRuin;
				}

				var allSinceLastCum = (await WheelOutcomeExtension.GetLastOutcomesUntil(lastCum, context)).ToList();

				var denialSinceLastCum = allSinceLastCum.Where(cOutcome => cOutcome.Type == (int)UserSetting.Outcome.Denial);
				var taskSinceLastCum = allSinceLastCum.Where(cOutcome => cOutcome.Type == (int)UserSetting.Outcome.Task);
				var edgeSinceLastCum = allSinceLastCum.Where(cOutcome => cOutcome.Type == (int)UserSetting.Outcome.Edge);

				cumScore = (denialSinceLastCum.Count() * 6 + taskSinceLastCum.Count() * 2 + edgeSinceLastCum.Count()) / 6f;
			}

			return cumScore;
		}

		private async Task<UserSetting.Outcome> CheckDenial(CommandContext ctx, Users user, UserSetting.Outcome outcome)
		{
			if (user.DenialTime != null && user.DenialTime > DateTime.Now)
			{
				if (outcome.HasFlag(UserSetting.Outcome.Orgasm)
					|| outcome.HasFlag(UserSetting.Outcome.Ruin))
				{
					await ctx.RespondAsync(
						"Haha, I would\'ve let you cum this time, but since you\'re still denied, "
						+ $"that won't happen {DiscordEmoji.FromName(ctx.Client, Config.Emojis.Blush)}.\n" +
						"As a punishment, you\'re gonna do your Task anyways though:");
				}
				else
				{
					await ctx.RespondAsync(
						"Well, i told you, that you\'d be denied now.\n"
						+ "You still want to do something? Hmm... let's see...");
				}

				outcome = UserSetting.Outcome.Denial | UserSetting.Outcome.Edge;
			}

			return outcome;
		}

		private async Task<bool> CheckLocked(CommandContext ctx, Users user, DiscordContext context)
		{
			if (user.LockTime != null && user.LockTime > DateTime.Now)
			{
				TimeSpan? timeUntilFree = user.LockTime - DateTime.Now;

				TimeSpan newTimeUntilFree =
					TimeSpan.FromTicks(timeUntilFree.Value.Ticks * Helpers.RandomGenerator.RandomInt(1, 3));

				if (newTimeUntilFree > TimeSpan.FromDays(365))
				{
					_ = ctx.RespondAsync("Fuck off");
					return false;
				}

				var responseText = "Oho, it seems like I told you to stay away from spinning the wheel...\n" +
								   $"That means you get some more extra time of no spinning {DiscordEmoji.FromName(ctx.Client, Config.Emojis.Blush)}";

				if (int.TryParse(user.LockReason, out int reasonInt))
				{
					var reason = (UserSetting.LockReason)reasonInt;

					switch (reason)
					{
						case UserSetting.LockReason.Cooldown:
							responseText =
								"Oho, it seems like I told you to stay away from spinning the wheel...\n" +
								$"That means you get some more extra time of no spinning {DiscordEmoji.FromName(ctx.Client, Config.Emojis.Blush)}";
							user.LockTime += newTimeUntilFree;
							break;

						case UserSetting.LockReason.Extension:
							responseText =
								"Hey! I already told you, that you'd get a longer lock on the Wheel! You still want more? Sure!";
							user.LockTime += newTimeUntilFree / 2;
							break;

						case UserSetting.LockReason.Task:
							responseText = "Haha, there's no way you were able to finish your Task so quickly. Do your Task, " +
										   "and then I'll give you another minute to think about your Actions.";
							user.LockTime += TimeSpan.FromMinutes(1);
							break;
					}
				}

				user.LockReason = ((int)UserSetting.LockReason.Extension).ToString();

				await ctx.RespondAsync(responseText);

				await context.SaveChangesAsync();

				return false;
			}
			else
			{
				return true;
			}
		}

		private async Task CheckUserHasOrgasm(CommandContext ctx, UserSetting.Outcome outcome)
		{
			if (outcome == UserSetting.Outcome.Orgasm || outcome == UserSetting.Outcome.Ruin)
			{
				var m = await ctx.Client.GetInteractivity().WaitForMessageAsync(
							x => x.Channel.Id == ctx.Channel.Id && x.Author.Id == ctx.Member.Id
																&& x.Content.Contains(@"//ruin") || x.Content.Contains(@"//ruined") || x.Content.Contains(@"//cum") || x.Content.Contains(@"//came"),
							TimeSpan.FromSeconds(120));

				if (m.TimedOut)
				{
					string text = outcome == UserSetting.Outcome.Ruin ? "ruin it like a good boy" : "cum in the end";

					var builder = new DiscordEmbedBuilder()
					{
						Title = $"Did you {text}?",
						Description = $"{ctx.Message.Author.Mention} You didn't tell me, if you properly did {text} :(" + Environment.NewLine
									+ "Please use either ``//ruined`` or ``//came`` depending on what you did"
					};

					await ctx.RespondAsync(embed: builder.Build());
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <returns>true, if checks are passed</returns>
		private async Task<bool> DoInitialCheck(CommandContext ctx)
		{
			using DiscordContext context = new DiscordContext();
			Users user = await UserExtension.GetUser(Convert.ToInt64(ctx.Message.Author.Id), context);
			SabrinaSettings sabrinaSettings = await context.SabrinaSettings.FindAsync(Convert.ToInt64(ctx.Guild.Id));

			if (sabrinaSettings == null)
			{
				sabrinaSettings = new SabrinaSettings
				{
					GuildId = Convert.ToInt64(ctx.Guild.Id),
					WheelChannel = Convert.ToInt64(ctx.Channel.Id)
				};

				await context.SabrinaSettings.AddAsync(sabrinaSettings);
				await context.SaveChangesAsync();
			}

			if (sabrinaSettings.WheelChannel == null)
			{
				sabrinaSettings.WheelChannel = Convert.ToInt64(ctx.Channel.Id);
				await context.SaveChangesAsync();
			}

			if (Convert.ToInt64(ctx.Channel.Id) != sabrinaSettings.WheelChannel.Value)
			{
				DiscordChannel channel =
					await ctx.Client.GetChannelAsync(Convert.ToUInt64(sabrinaSettings.WheelChannel));

				await ctx.RespondAsync(
					$"You cannot issue this command from this Channel. Please use {channel.Mention}");
				return false;
			}

			return true;
		}

		private async Task<UserSetting.Outcome> GetOutcome(CommandContext ctx, DiscordContext context)
		{
			Users user = await UserExtension.GetUser(Convert.ToInt64(ctx.Message.Author.Id), context);
			var wheelMode = await WheelSettingExtension.GetMode(ctx.Guild.Id, context);

			var wheelDifficultySetting = await UserSetting.GetSettingAsync(user.UserId, UserSetting.SettingID.WheelDifficulty, context);
			var difficulty = UserSettingExtensions.GetValue<UserSetting.WheelDifficultyPreference>(wheelDifficultySetting);

			WheelChances chances = await context.WheelChances.FindAsync((int)difficulty);

			var cumScore = await CalculateCumScore(context, user);
			var cumChance = Convert.ToInt32(cumScore * chances.Orgasm);

			if (wheelMode.HasFlag(WheelSettingExtension.WheelMode.NoCum))
			{
				cumChance = 0;
			}

			var ruinChance = Convert.ToInt32(cumScore * chances.Ruin);

			if (wheelMode.HasFlag(WheelSettingExtension.WheelMode.NoRuin))
			{
				ruinChance = 0;
			}

			var denialChance = chances.Denial;

			if (wheelMode.HasFlag(WheelSettingExtension.WheelMode.NoDenial))
			{
				denialChance = 0;
			}

			var outcomeChanceValue = Helpers.RandomGenerator.RandomInt(
				0,
				denialChance + chances.Task + chances.Edge + ruinChance + cumChance);
			UserSetting.Outcome outcome;
			if (outcomeChanceValue < denialChance)
			{
				outcome = UserSetting.Outcome.Denial;
			}
			else if (outcomeChanceValue < denialChance + chances.Edge)
			{
				outcome = UserSetting.Outcome.Edge;
			}
			else if (outcomeChanceValue < denialChance + chances.Edge + chances.Task)
			{
				outcome = UserSetting.Outcome.Task;
			}
			else if (outcomeChanceValue < denialChance + chances.Edge + chances.Task + cumChance)
			{
				outcome = UserSetting.Outcome.Ruin;
			}
			else
			{
				outcome = UserSetting.Outcome.Orgasm;
			}

			return outcome;
		}

		private async Task<WheelOutcome> GetWheelOutcome(DiscordContext context, Users user, UserSetting.Outcome outcome)
		{
			WheelOutcome wheelOutcome = null;
			var userItems = await WheelItemExtension.GetUserItemsAsync(user.UserId, context);

			while (wheelOutcome == null)
			{
				try
				{
					wheelOutcomes = ReflectiveEnumerator.GetEnumerableOfType<WheelOutcome>(outcome, UserSetting.GetAllSettings(user.UserId, context).ToDictionary(setting => (UserSetting.SettingID)setting.SettingId), userItems, _services)
						.ToList();
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}

				foreach (var wheeloutcome in wheelOutcomes)
				{
					await wheeloutcome.BuildAsync();
				}

				wheelOutcomes = wheelOutcomes.Where(e => !e.Outcome.HasFlag(UserSetting.Outcome.NotSet)).ToList();

				if (wheelOutcomes.Count < 1)
				{
					continue;
				}

				// Choose an outcome by summing up the chance values of all possible outcomes and
				// then generating a random number inside those.
				var combinedChance = 0;

				foreach (WheelOutcome currentOutcome in wheelOutcomes) combinedChance += currentOutcome.Chance;

				var chance = 0;
				var minChance = Helpers.RandomGenerator.RandomInt(0, combinedChance);

				foreach (WheelOutcome currentOutcome in wheelOutcomes)
				{
					chance += currentOutcome.Chance;
					if (minChance < chance)
					{
						wheelOutcome = currentOutcome;
						break;
					}
				}
			}

			return wheelOutcome;
		}

		/// <summary>
		/// Loads a random Line from a TextDocument
		/// </summary>
		/// <param name="file">The file path.</param>
		/// <returns>The <see cref="Task"/>.</returns>
		private async Task<string> LoadLineAsync(string file)
		{
			using StreamReader reader = File.OpenText(file);
			var fileText = await reader.ReadToEndAsync();
			string[] lines = fileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			return lines[Helpers.RandomGenerator.RandomInt(0, lines.Length)];
		}

		private async Task PunishmentInternal(CommandContext ctx)
		{
			using var context = new DiscordContext();
			var user = await UserExtension.GetUser(ctx.Message.Author.Id);
			var settings = UserSetting.GetAllSettings(Convert.ToInt64(ctx.User.Id), context).ToDictionary(setting => (UserSetting.SettingID)setting.SettingId);
			var items = await WheelItemExtension.GetUserItemsAsync(user.UserId, context);
			var punishmentModules = PunishmentModule.GetAll(settings, items.ToList());

			var sumChances = punishmentModules.Sum(cModule => cModule.Chance);

			var chanceValue = Helpers.RandomGenerator.RandomInt(0, sumChances);

			int cChance = 0;
			PunishmentModule module = null;
			DiscordEmbed embed = null;

			foreach (var cModule in punishmentModules)
			{
				cChance += cModule.Chance;

				if (chanceValue < cChance)
				{
					module = cModule;
					break;
				}
			}

			if (module == null)
			{
				DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
				{
					Title = "No suitable punishment found :(",
					Description = "Please set up your preferences with ``//settings setup``. You should at least enable cbt and set up some bondage gear."
				};
				await ctx.RespondAsync(embed: builder.Build());
				return;
			}

			await module.Generate();
			embed = module.Embed;

			if (module.WheelLockTime != null && user.LockTime <= DateTime.Now)
			{
				user.LockTime += module.WheelLockTime;

				var builder = new DiscordEmbedBuilder(embed);
				builder.Description += " You are not allowed to re-roll until then.";
				embed = builder.Build();
			}

			if (module.WheelLockTime != null && user.DenialTime <= DateTime.Now)
			{
				user.DenialTime += module.DenialTime;
			}

			await ctx.RespondAsync(embed: embed);

			await context.SaveChangesAsync();
		}
	}
}