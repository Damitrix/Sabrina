// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Save-Instructions.cs" company="SalemsTools">
//     Do whatever
// </copyright>
// <summary>
// Defines the SlaveInstructions type.
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
	using SkiaSharp;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// The slave instructions command group.
	/// </summary>
	public class SlaveInstructions : BaseCommandModule
	{
		private readonly DiscordContext _context;

		public SlaveInstructions()
		{
			_context = new DiscordContext();
		}

		/// <summary>
		/// The get reports Command.
		/// </summary>
		/// <param name="ctx">The Command Context.</param>
		/// <param name="time">The time in which to get reports.</param>
		/// <returns>The <see cref="Task"/>.</returns>
		[Command("getreports")]
		[Description("Get all Reports in a specified time")]
		[RequireRoles(RoleCheckMode.Any, "mistress", "Mistress", "master", "Master", "dom", "Dom", "domme", "Domme")]
		public async Task GetReportsAsync(CommandContext ctx, string type, string time)
		{
			type = type.ToUpperInvariant();
			time = time.ToUpperInvariant();
			TimeSpan timespan;

			try
			{
				timespan = TimeResolver.GetTimeSpan(time);
			}
			catch (InvalidCastException ex)
			{
				await ctx.RespondAsync(ex.Message);
				return;
			}

			var earliestTime = DateTime.Now - timespan;
			var reports = _context.Slavereports.Where(report => report.TimeOfReport > earliestTime).OrderBy(r => r.TimeOfReport).ToArray();

			switch (type)
			{
				case "DIAGRAM":
					var groupedReports = reports.GroupBy(r => r.UserId);

					var cSubGroup = groupedReports.Take(5);
					int skipped = 5;

					while (cSubGroup.Count() > 0)
					{
						var diagram = await GenerateDiagram(ctx, cSubGroup, timespan);
						using var stream = new MemoryStream(diagram);
						await ctx.RespondWithFileAsync("Diagram.png", stream);
						cSubGroup = groupedReports.Skip(skipped).Take(5);
						skipped += 5;
					}
					break;

				case "TEXT":
					await GetReportsText(ctx, reports);
					break;

				default:
					await ctx.RespondAsync("Invalid type.\n Usage: ``//getreports diagram 5d``.\nPossible types: ``diagram``, ``text``");
					break;
			}
		}

		/// <summary>
		/// The report Command.
		/// </summary>
		/// <param name="ctx">The Command Context.</param>
		/// <param name="outcome">The outcome the user got.</param>
		/// <param name="edges">The edges the user did.</param>
		/// <param name="time">The time it took for the Task.</param>
		/// <returns>The <see cref="Task"/>.</returns>
		[Command("report")]
		[Description("Report your daily tasks to your Mistress")]
		public async Task ReportAsync(
			CommandContext ctx,
			[Description("Your outcome (denial/ruin/orgasm)")]
			string outcome = null,
			[Description("How many edges it took")]
			int edges = 0,
			[Description("How long it took (5m = 5 minutes | 5m12s = 5 minutes, 12 seconds)")]
			string time = null)
		{
			// TODO: Check for Channel

			if (outcome == null || time == null)
			{
				await ctx.RespondAsync("Please supply information about your report (``//report denial 5 12m5s`` to report denial after 5 edges and 12 minutes and 5 seconds)");
				return;
			}

			outcome = outcome.ToUpperInvariant();

			IQueryable<Slavereports> slaveReports =
				from report in _context.Slavereports.Where(report => report.TimeOfReport > DateTime.Now.AddHours(-16))
				where report.UserId == Convert.ToInt64(ctx.User.Id)
				select report;

			var lastReport = slaveReports.FirstOrDefault();

			if (lastReport != null)
			{
				await ctx.RespondAsync(
					$"You can only report once every 20 hours. You can report again in {TimeResolver.TimeToString(lastReport.TimeOfReport.AddHours(20) - DateTime.Now)}");
				var dm = await (await ctx.Guild.GetMemberAsync(Config.Users.Aki)).CreateDmChannelAsync();
				await dm.SendMessageAsync(
					$"{ctx.Message.Author} has reported {TimeResolver.TimeToString(lastReport.TimeOfReport.AddHours(20) - DateTime.Now)} too early.");
				return;
			}

			if (Enum.TryParse(outcome, true, out UserSetting.Outcome result))
			{
				TimeSpan span;
				try
				{
					var user = await UserExtension.GetUser(ctx.Message.Author.Id);
					span = TimeResolver.GetTimeSpan(time);

					await Task.Run(
						async () =>
							{
								var report = new Slavereports()
								{
									TimeOfReport = DateTime.Now,
									UserId = user.UserId,
									Edges = edges,
									TimeSpan = span.Ticks,
									SessionOutcome = outcome
								};

								_context.Slavereports.Add(report);

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

				foreach (string possibleOutcome in Enum.GetNames(typeof(UserSetting.Outcome)))
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
			var parsedOutcome = Enum.Parse<UserSetting.Outcome>(outcome, true);

			var responseColor = DiscordColor.Green;

			if (parsedOutcome == UserSetting.Outcome.Denial)
			{
				Tuple<string, string>[] templates =
					{
						new Tuple<string, string>("Hehe, looks like that's it for you for today, ", " ^^"),
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

			if (parsedOutcome == UserSetting.Outcome.Ruin)
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

			if (parsedOutcome == UserSetting.Outcome.Orgasm)
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
					Text = "You can report back in 16 hours"
				}
			};

			await ctx.RespondAsync(embed: responseBuilder.Build());
		}

		private static async Task GetReportsText(CommandContext ctx, Slavereports[] reports)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("Name");
			builder.Append(",");
			builder.Append("Date");
			builder.Append(",");
			builder.Append("Outcome");
			builder.Append(",");
			builder.Append("# of Edges");
			builder.Append(",");
			builder.Append("Time");
			builder.AppendLine();

			Dictionary<DiscordUser, DiscordMember> members = new Dictionary<DiscordUser, DiscordMember>();

			foreach (var report in reports)
			{
				if (!members.Any(m => m.Key.Id == Convert.ToUInt64(report.UserId)))
				{
					var cUser = await ctx.Client.GetUserAsync(Convert.ToUInt64(report.UserId));
					DiscordMember cMember = null;
					try
					{
						cMember = await ctx.Guild.GetMemberAsync(cUser.Id);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Cannot get Member for {cUser.Username} ({cUser.Id})");
						Console.WriteLine(ex.Message);
					}
					members.Add(cUser, cMember);
				}

				var member = members.First(m => m.Key.Id == Convert.ToUInt64(report.UserId));
				builder.Append(member.Value != null && member.Value.Nickname != null ? member.Value.Nickname : member.Key.Username);
				builder.Append(",");
				builder.Append(report.TimeOfReport.ToString("dd.MM.yyy hh:mm:ss"));
				builder.Append(",");
				builder.Append(report.SessionOutcome.ToLowerInvariant());
				builder.Append(",");
				builder.Append(report.Edges);
				builder.Append(",");
				builder.Append(TimeSpan.FromTicks(report.TimeSpan).ToString("hh\\:mm\\:ss"));
				builder.AppendLine();
			}

			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(builder.ToString()));
			await ctx.RespondWithFileAsync("Export.csv", stream);
		}

		private static string[] SplitLine(SKPaint paint, float maxWidth, string text, float spaceWidth)
		{
			var result = new List<string>();

			var words = text.Split(new[] { " " }, StringSplitOptions.None);

			var line = new System.Text.StringBuilder();
			float width = 0;
			foreach (var word in words)
			{
				var wordWidth = paint.MeasureText(word);
				var wordWithSpaceWidth = wordWidth + spaceWidth;
				var wordWithSpace = word + " ";

				if (width + wordWidth > maxWidth)
				{
					result.Add(line.ToString());
					line = new System.Text.StringBuilder(wordWithSpace);
					width = wordWithSpaceWidth;
				}
				else
				{
					line.Append(wordWithSpace);
					width += wordWithSpaceWidth;
				}
			}

			result.Add(line.ToString());

			return result.ToArray();
		}

		private void DrawTextArea(SKCanvas canvas, SKPaint paint, float x, float y, float maxWidth, float lineHeight, string text)
		{
			var spaceWidth = paint.MeasureText(" ");
			var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			lines = lines.SelectMany(l => SplitLine(paint, maxWidth, l, spaceWidth)).ToArray();

			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i];
				canvas.DrawText(line, x, y, paint);
				y += lineHeight;
			}
		}

		private async Task<byte[]> GenerateDiagram(CommandContext ctx, IEnumerable<IGrouping<long, Slavereports>> groupedReports, TimeSpan timespan)
		{
			if (groupedReports.Count() > 5)
			{
				throw new ArgumentOutOfRangeException("Maximum allowed number of users in a diagram is 5");
			}

			const int ImageWidth = 1024;
			const int ImageHeight = 800;
			const int DiagramSpacingLeft = 90; // Spacing on the left of the Y line
			const int DiagramSpacingBottom = 120; // Spacing below the X line

			SKBitmap bmp = new SKBitmap(ImageWidth, ImageHeight);
			using SKCanvas canvas = new SKCanvas(bmp);

			// Set black background
			canvas.Clear(new SKColor(47, 49, 54));

			// Create all colors and strokes needed later on
			SKPaint diagramPaint = new SKPaint()
			{
				Color = new SKColor(200, 200, 230),
				StrokeWidth = 3,
				IsAntialias = true,
			};

			SKPaint diagramPaintThicc = new SKPaint()
			{
				Color = new SKColor(200, 200, 230),
				StrokeWidth = 6,
				IsAntialias = true
			};

			SKPaint textPaint = new SKPaint()
			{
				Color = new SKColor(200, 200, 230),
				TextSize = 18f,
				TextAlign = SKTextAlign.Center,
				IsAntialias = true,
				IsEmbeddedBitmapText = true
			};

			SKPaint leftTextPaint = new SKPaint()
			{
				Color = new SKColor(200, 200, 230),
				TextSize = 18f,
				TextAlign = SKTextAlign.Right,
				IsAntialias = true,
				IsEmbeddedBitmapText = true
			};

			// Draw X and Y lines
			canvas.DrawLine(new SKPoint(DiagramSpacingLeft, 0), new SKPoint(DiagramSpacingLeft, ImageHeight), diagramPaint);
			canvas.DrawLine(new SKPoint(0, ImageHeight - DiagramSpacingBottom), new SKPoint(ImageWidth, ImageHeight - DiagramSpacingBottom), diagramPaint);

			const int rows = 8;

			// max and min time of all reports
			var maxTime = groupedReports.Max(g => g.Max(r => r.TimeSpan));
			var minTime = groupedReports.Min(g => g.Min(r => r.TimeSpan));

			var timeDiff = TimeSpan.FromTicks(maxTime - minTime);
			var individualTime = TimeSpan.FromTicks(maxTime / rows);
			var individualRowSpacing = Convert.ToInt32((ImageHeight - DiagramSpacingBottom * 1.4f) / rows);
			var individualColumnSpacing = Convert.ToInt32((ImageWidth - DiagramSpacingLeft * 1.4f) / (timespan.TotalDays + 1));

			for (int i = 1; i <= rows; i++)
			{
				var y = ImageHeight - DiagramSpacingBottom - individualRowSpacing * i;
				canvas.DrawLine(new SKPoint(DiagramSpacingLeft - 10, y), new SKPoint(DiagramSpacingLeft + 10, y), diagramPaintThicc);
				canvas.DrawText((individualTime * i).ToString("hh\\:mm\\:ss"), new SKPoint(DiagramSpacingLeft - 10, y - 10), leftTextPaint);
			}

			var xLineHeight = ImageHeight - DiagramSpacingBottom;

			for (int i = 1; i <= timespan.TotalDays + 2; i++)
			{
				var x = DiagramSpacingLeft + individualColumnSpacing * i;
				canvas.DrawLine(new SKPoint(x, xLineHeight - 10), new SKPoint(x, xLineHeight + 10), diagramPaintThicc);
				canvas.DrawText((DateTime.Now - timespan + TimeSpan.FromDays(1) + TimeSpan.FromDays(i - 1)).ToString("dd.MM."), new SKPoint(x, xLineHeight + 30), textPaint);
			}

			// Create a color for each user
			SKColor[] userColors = new SKColor[]
			{
					new SKColor(36, 123,160),
					new SKColor(112,193,179),
					new SKColor(178, 219, 191),
					new SKColor(243,255,189),
					new SKColor(225,22,84)
			};

			for (int i = 0; i < groupedReports.Count(); i++)
			{
				var group = groupedReports.ElementAt(i);
				var color = userColors[i];
				var colorBright = new SKColor((byte)(color.Red + 100 > 255 ? 255 : color.Red + 100), (byte)(color.Green + 100 > 255 ? 255 : color.Green + 100), (byte)(color.Blue + 100 > 255 ? 255 : color.Blue + 100));

				var paint = new SKPaint()
				{
					Color = color,
					StrokeWidth = 3,
					TextSize = 18f,
					TextAlign = SKTextAlign.Center,
					IsAntialias = true,
					IsEmbeddedBitmapText = true
				};

				var paintBright = new SKPaint()
				{
					Color = colorBright,
					StrokeWidth = 3,
					TextSize = 18f,
					TextAlign = SKTextAlign.Center,
					IsAntialias = true,
					IsEmbeddedBitmapText = true
				};

				SKPoint lastCoord = SKPoint.Empty;

				var user = await ctx.Client.GetUserAsync(Convert.ToUInt64(group.Key));
				var spacePerUser = (ImageWidth - DiagramSpacingLeft) / 5;

				// Get the avatar
				HttpClient httpClient = new HttpClient();
				var userImgResponse = await httpClient.GetAsync(user.AvatarUrl);

				// Only add the user avatar if it can be downloaded
				if (userImgResponse.IsSuccessStatusCode)
				{
					DiscordMember member = null;
					try
					{
						member = await ctx.Channel.Guild.GetMemberAsync(user.Id);
					}
					catch (Exception)
					{ }

					var antialiasedPaint = new SKPaint()
					{
						IsAntialias = true
					};

					textPaint.GetFontMetrics(out SKFontMetrics metrics);
					var textSpacing = Convert.ToInt32(metrics.XMax + (metrics.XMax / 4));

					var x = DiagramSpacingLeft + 5 + spacePerUser * i;
					var y = ImageHeight - DiagramSpacingBottom + textSpacing;
					var fullSpace = DiagramSpacingBottom - textSpacing < spacePerUser ? DiagramSpacingBottom : spacePerUser;
					var halfSpace = fullSpace / 2;

					var left = halfSpace / 4 * 1;
					var top = halfSpace / 4 * 1;
					var size = halfSpace / 4 * 3;

					var baseBmp = new SKBitmap(spacePerUser, fullSpace - Convert.ToInt32(textSpacing));
					var baseCanvas = new SKCanvas(baseBmp);
					baseCanvas.Clear(color);

					var userImgBmp = SKBitmap.Decode(await userImgResponse.Content.ReadAsStreamAsync());
					var clippedBmp = new SKBitmap(userImgBmp.Width, userImgBmp.Height);
					SKCanvas userImgCanvas = new SKCanvas(clippedBmp);
					userImgCanvas.ClipRoundRect(new SKRoundRect(new SKRect(0, 0, userImgBmp.Width, userImgBmp.Height), 100, 100));
					userImgCanvas.DrawBitmap(userImgBmp, 0, 0, antialiasedPaint);
					userImgCanvas.Flush();

					baseCanvas.DrawBitmap(clippedBmp, new SKRect(left, top, left + size, top + size), antialiasedPaint);

					SKPaint namePaint = new SKPaint()
					{
						Color = new SKColor(47, 49, 54),
						TextAlign = SKTextAlign.Left,
						TextSize = 18,
						IsAntialias = true,
						IsEmbeddedBitmapText = true,
					};

					DrawTextArea(baseCanvas, namePaint, left * 2 + size, top * 2, spacePerUser - left * 2 + size, 15, member?.Nickname ?? user.Username);

					canvas.DrawBitmap(baseBmp, new SKPoint(x, y), antialiasedPaint);
				}

				foreach (var report in group)
				{
					var minDate = (DateTime.Now - timespan).Date;
					var maxDate = (DateTime.Now + TimeSpan.FromDays(1)).Date;
					var totalSpace = individualColumnSpacing * (timespan.TotalDays + 1);
					var percentPointPerTick = (float)totalSpace / (maxDate - minDate).Ticks;
					var x = DiagramSpacingLeft + (percentPointPerTick * (report.TimeOfReport - minDate).Ticks); //Subtract the time of the report from the mindate, to get the time relative from the beginning of the graph. multiply by percentagepoint of a tick on the graph. add spacing,
					var y = ImageHeight - DiagramSpacingBottom - ((ImageHeight - (DiagramSpacingBottom * 1.4f)) / Convert.ToSingle(maxTime) * report.TimeSpan);
					var coord = new SKPoint(x, y);
					canvas.DrawCircle(coord, 8, paint);
					canvas.DrawText(TimeSpan.FromTicks(report.TimeSpan).ToString(), new SKPoint(coord.X, coord.Y - 10), paintBright);
					//canvas.DrawText(report.TimeOfReport.ToString(), new SKPoint(coord.X, coord.Y + 15), paintBright);

					if (lastCoord != SKPoint.Empty)
					{
						canvas.DrawLine(lastCoord, coord, paint);
					}

					lastCoord = coord;
				}
			}

			using var ms = new MemoryStream();
			using SKManagedWStream skStream = new SKManagedWStream(ms);
			if (SKPixmap.Encode(skStream, bmp, SKEncodedImageFormat.Png, 100))
			{
				return ms.ToArray();
			}

			return Array.Empty<byte>();
		}
	}
}