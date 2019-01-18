// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Save-Instructions.cs" company="SalemsTools">
//   Do whatever
// </copyright>
// <summary>
//   Defines the SlaveInstructions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Configuration;

namespace Sabrina.Commands
{
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Sabrina.Entities;
    using Sabrina.Models;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// The slave instructions command group.
    /// </summary>
    internal class SlaveInstructions
    {
        private DiscordContext _context;

        public SlaveInstructions()
        {
            _context = new DiscordContext();
        }

        /// <summary>
        /// The get reports Command.
        /// </summary>
        /// <param name="ctx">
        /// The Command Context.
        /// </param>
        /// <param name="time">
        /// The time in which to get reports.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("getreports")]
        [Description("Get all Reports in a specified time")]
        [RequireRolesAttribute("mistress")]
        public async Task GetReportsAsync(CommandContext ctx, string time)
        {
            TimeSpan timespan;

            try
            {
                timespan = TimeResolver.GetTimeSpan(time);
            }
            catch (InvalidCastException)
            {
                await ctx.RespondAsync("Invalid TimeSpan");
                return;
            }

            try
            {
                var reports = _context.Slavereports.Where(report => report.TimeOfReport > DateTime.Now - timespan);

                List<Tuple<long, List<Slavereports>>> groupedReports =
                    new List<Tuple<long, List<Slavereports>>>();

                foreach (var report in reports)
                {
                    if (groupedReports.Any(rep => rep.Item1 == report.UserId))
                    {
                        groupedReports.First(rep => rep.Item1 == report.UserId).Item2.Add(report);
                    }
                    else
                    {
                        var list = new List<Slavereports> { report };

                        groupedReports.Add(
                            new Tuple<long, List<Slavereports>>(report.UserId, list));
                    }
                }

                foreach (Tuple<long, List<Slavereports>> userReports in groupedReports)
                {
                    var currentUser = await ctx.Client.GetUserAsync(Convert.ToUInt64(userReports.Item1));
                    var builder = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = currentUser.Username,
                            IconUrl = currentUser.AvatarUrl
                        }
                    };

                    builder.AddField("Sum of edges", userReports.Item2.Sum(report => report.Edges).ToString());

                    builder.AddField("Average edges", userReports.Item2.Average(report => report.Edges).ToString(CultureInfo.InvariantCulture));

                    builder.AddField(
                        "Sum of time",
                        new TimeSpan(userReports.Item2.Sum(report => report.TimeSpan)).ToString());

                    builder.AddField(
                        "Average time",
                        new TimeSpan(Convert.ToInt64(userReports.Item2.Average(report => report.TimeSpan))).ToString());

                    builder.AddField(
                        "Denials",
                        userReports.Item2.Count(report => report.SessionOutcome == "denial").ToString());

                    builder.AddField(
                        "Ruins",
                        userReports.Item2.Count(report => report.SessionOutcome == "ruin").ToString());

                    builder.AddField(
                        "Orgasms",
                        userReports.Item2.Count(report => report.SessionOutcome == "orgasm").ToString());

                    await ctx.RespondAsync(embed: builder.Build());
                }
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync(ex.Message);

                int currentLength = 0;

                while (currentLength < ex.StackTrace.Length)
                {
                    int lengthLeft = ex.StackTrace.Length - currentLength;
                    int subVal = lengthLeft < 2000 ? lengthLeft : 1999;

                    await ctx.RespondAsync(ex.StackTrace.Substring(currentLength, subVal));
                    currentLength += subVal;
                }

                if (ex.InnerException != null)
                {
                    await ctx.RespondAsync(ex.InnerException.Message.Substring(0, 1999));
                }
            }
        }

        /// <summary>
        /// The report Command.
        /// </summary>
        /// <param name="ctx">
        /// The Command Context.
        /// </param>
        /// <param name="outcome">
        /// The outcome the user got.
        /// </param>
        /// <param name="edges">
        /// The edges the user did.
        /// </param>
        /// <param name="time">
        /// The time it took for the Task.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("report")]
        [Description("Report your daily tasks to your Mistress")]
        public async Task ReportAsync(
            CommandContext ctx,
            [Description("Your outcome (denial/ruin/orgasm)")]
            string outcome,
            [Description("How many edges it took")]
            int edges,
            [Description("How long it took (5m = 5 minutes | 5m12s = 5 minutes, 12 seconds)")]
            string time)
        {
            if (ctx.Channel.Name != Config.Channels.Instruction)
            {
                await ctx.RespondAsync($"Please report only in the {Config.Channels.Instruction} channel");
                return;
            }

            IQueryable<Slavereports> slaveReports =
                from report in _context.Slavereports.Where(report => report.TimeOfReport > DateTime.Now.AddHours(-20))
                where report.UserId == Convert.ToInt64(ctx.User.Id)
                select report;

            if (slaveReports.Any() && Convert.ToInt64(ctx.Message.Author.Id) != 347004618183540740)
            {
                var lastReport = slaveReports.First();

                if (lastReport != null)
                {
                    await ctx.RespondAsync(
                        $"You can only report once every 20 hours. You can report again in {TimeResolver.TimeToString(lastReport.TimeOfReport.AddHours(20) - DateTime.Now)}");
                    var dm = await (await ctx.Guild.GetMemberAsync(Config.Users.Aki)).CreateDmChannelAsync();
                    await dm.SendMessageAsync(
                        $"{ctx.Message.Author} has reported {TimeResolver.TimeToString(lastReport.TimeOfReport.AddHours(20) - DateTime.Now)} too early.");
                    return;
                }
            }
            else if (Convert.ToInt64(ctx.Message.Author.Id) == 347004618183540740)
            {
                IQueryable<Slavereports> slaveReportsPj =
                    from report in _context.Slavereports.Where(report => report.TimeOfReport > DateTime.Now.AddHours(-8))
                    where report.UserId == Convert.ToInt64(ctx.User.Id)
                    select report;

                if (slaveReportsPj.Any())
                {
                    var lastReport = slaveReportsPj.First();

                    if (lastReport != null)
                    {
                        await ctx.RespondAsync(
                            $"You can only report once every 8 hours. You can report again in {TimeResolver.TimeToString(lastReport.TimeOfReport.AddHours(8) - DateTime.Now)}");
                        var dm = await (await ctx.Guild.GetMemberAsync(Config.Users.Aki)).CreateDmChannelAsync();
                        await dm.SendMessageAsync(
                            $"{ctx.Message.Author} has reported {TimeResolver.TimeToString(lastReport.TimeOfReport.AddHours(8) - DateTime.Now)} too early.");
                        return;
                    }
                }
            }

            if (Enum.TryParse(outcome, out WheelExtension.Outcome result))
            {
                TimeSpan span;
                try
                {
                    span = TimeResolver.GetTimeSpan(time);

                    await Task.Run(
                        async () =>
                            {
                                var report = new Slavereports()
                                {
                                    TimeOfReport = ctx.Message.Timestamp.LocalDateTime,
                                    UserId = Convert.ToInt64(Convert.ToInt64(ctx.Message.Author.Id)),
                                    Edges = edges,
                                    TimeSpan = span.Ticks,
                                    SessionOutcome = outcome
                                };

                                await _context.SaveChangesAsync();
                            });
                }
                catch
                {
                    var builder = new DiscordEmbedBuilder
                    {
                        Title = "Error",
                        Description =
                                              "That's not how this works, you can enter your time in one of the following formats:",
                        Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "You get the Idea..." }
                    };

                    builder.AddField("1h5m12s", "1 hour, 5 minutes, 12 seconds");
                    builder.AddField("5m", "5 minutes");
                    builder.AddField("2h", "2 hours");
                    builder.AddField("1200s", "1200 seconds");

                    await ctx.RespondAsync(embed: builder.Build());
                    return;
                }
            }
            else
            {
                var builder = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "That's not how this works, you gotta use one of the following:"
                };

                foreach (string possibleOutcome in Enum.GetNames(typeof(WheelExtension.Outcome)))
                {
                    builder.AddField(possibleOutcome, $"``//report {possibleOutcome} {edges} {time}``");
                }

                await ctx.RespondAsync(embed: builder.Build());
                return;
            }

            var formatText = "{0}{1}{2}";
            var prefix = "Hey, ";
            var postfix = ". Thanks for reporting your task.";
            string name = ctx.User.Mention;

            var responseColor = DiscordColor.Green;

            if (outcome == WheelExtension.Outcome.Denial.ToString())
            {
                Tuple<string, string>[] templates =
                    {
                        new Tuple<string, string>("hihi, looks like that's it for you, for today, ", " ^^"),
                        new Tuple<string, string>(
                            "That's what i like to see, ",
                            ". Now, can you do me a favor and report that again next time?~"),
                        new Tuple<string, string>(
                            "Ohh, is little ",
                            " denied? Well, too bad, you'll have to wait for your next chance~"),
                        new Tuple<string, string>(
                            "1, 2, 3, 4, ",
                            " is denied! ...Were you expecting a rhyme? Sucks being you, then.")
                    };

                Tuple<string, string> template = templates[Helpers.RandomGenerator.RandomInt(0, templates.Length)];
                prefix = template.Item1;
                postfix = template.Item2;

                responseColor = DiscordColor.Red;
            }

            if (outcome == WheelExtension.Outcome.Ruin.ToString())
            {
                Tuple<string, string>[] templates =
                    {
                        new Tuple<string, string>(
                            "Hmm, better than nothing, right, ",
                            "? Did it feel good?~ haha, of course not."),
                        new Tuple<string, string>(
                            "Oh ",
                            ", I don't know what i like more, denial or ruin... Do you think you get to deny yourself next time? :3"),
                        new Tuple<string, string>(
                            "It's not even a full orgasm, but our ",
                            " still followed Orders. I bet you'll be even more obedient with your next chance..."),
                        new Tuple<string, string>("Another ruined one for ", " . Check.")
                    };

                Tuple<string, string> template = templates[Helpers.RandomGenerator.RandomInt(0, templates.Length)];
                prefix = template.Item1;
                postfix = template.Item2;

                responseColor = DiscordColor.Yellow;
            }

            if (outcome == WheelExtension.Outcome.Orgasm.ToString())
            {
                Tuple<string, string>[] templates =
                    {
                        new Tuple<string, string>(
                            "Meh, ",
                            " got a full Orgasm. How boring. It hope you get a ruined one next time."),
                        new Tuple<string, string>(
                            "And Mistress allowed that? You got lucky, ",
                            ". But i think i should ask Mistress to ruin your next one."),
                        new Tuple<string, string>("... ", " ..."),
                        new Tuple<string, string>("Are you sure, you did it correctly, ", " ?")
                    };

                Tuple<string, string> template = templates[Helpers.RandomGenerator.RandomInt(0, templates.Length)];
                prefix = template.Item1;
                postfix = template.Item2;

                responseColor = DiscordColor.Green;
            }

            if (Convert.ToInt64(ctx.Message.Author.Id) == 347004618183540740 && DateTime.Now < new DateTime(2018, 9, 13))
            {
                if (outcome == WheelExtension.Outcome.Denial.ToString())
                {
                    prefix = "Enjoy your 21 days, ";
                    postfix = " . I'm sure you're having fun :)";
                }
                else
                {
                    prefix =
                        $"Ohoho, {(await ctx.Guild.GetMemberAsync(Config.Users.Aki)).Mention} isn't gonna enjoy that, ";
                    postfix = " .";
                }
            }

            var responseBuilder = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.User.Username,
                    IconUrl = ctx.User.AvatarUrl
                },
                Color = responseColor,
                Description = string.Format(formatText, prefix, name, postfix),
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "You can report back in 20 hours"
                }
            };

            if (Convert.ToInt64(ctx.Message.Author.Id) == 347004618183540740)
            {
                responseBuilder.Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = "You can report back in 8 hours"
                };
            }

            await ctx.RespondAsync(embed: responseBuilder.Build());
        }
    }
}