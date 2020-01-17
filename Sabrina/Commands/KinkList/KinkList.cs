using Configuration;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace Sabrina.Commands.KinkList
{
	public class KinkList
	{
		private static readonly string HashChars =
			"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.=+*^!@";

		private static readonly Int64 MaxSafeNumber = 9007199254740991;

		private enum Colors
		{
			NotEntered,
			Favorite,
			Like,
			Okay,
			Maybe,
			No,
		};

		[Command("getKink")]
		[Aliases("getkinks", "kink")]
		[Description("Get Stati of Kinks")]
		public async Task GetValueTask(CommandContext ctx, [Description("Mention your target here.")] DiscordUser user, [Description("Partial or full name of kink. Use \"\" to display all.")] string kinkName)
		{
			using var conn = new SqlConnection(Config.DatabaseConnectionString);
			await conn.OpenAsync();

			SqlCommand cmd = new SqlCommand();
			cmd = new SqlCommand($"SELECT (Hash) FROM KinkHashes WHERE UserID = @UserID", conn);

			cmd.Parameters.Add("@UserID", SqlDbType.Decimal).Value = user.Id;

			try
			{
				var reader = await cmd.ExecuteReaderAsync();
				if (reader.HasRows && await reader.ReadAsync())
				{
					int[] values = this.ParseHash(reader["hash"].ToString());
					List<KinkName> kinkNames = this.ParseKinksText(File.ReadAllText(Directory.GetCurrentDirectory() + "/Commands/KinkList/KinkList.txt"));

					List<DiscordEmbed> embeds = new List<DiscordEmbed>();
					DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

					string lastCategory = "";

					foreach (var kink in kinkNames)
					{
						if (kink.Name.ToLower().Contains(kinkName.ToLower()) || kink.Category.ToLower().Contains(kinkName.ToLower()) || kink.Column.ToLower().Contains(kinkName.ToLower()))
						{
							if (builder.Fields.Count >= 24)
							{
								embeds.Add(builder.Build());
								builder = new DiscordEmbedBuilder();
							}

							if (lastCategory != kink.Category)
							{
								var cEmbed = builder.Build();
								if (cEmbed.Fields != null && cEmbed.Fields.Count > 0)
								{
									embeds.Add(builder.Build());
									builder = new DiscordEmbedBuilder();
								}

								lastCategory = kink.Category;
								builder.AddField(kink.Category, "------------------------");
							}
							builder.AddField($"{kink.Column}: {kink.Name}",
								Enum.GetNames(typeof(Colors))[values[kinkNames.IndexOf(kink)]]);
						}
					}

					embeds.Add(builder.Build());

					if (embeds.Count > 0)
					{
						foreach (var embed in embeds)
						{
							await ctx.RespondAsync($"Here you go, these are all of {user.Username}'s kinks i found for for ``{kinkName}``", false, embed);
						}
					}
					else
					{
						await ctx.RespondAsync($"I cannot find any more values similar to ``{kinkName}`` for {user.Username}.");
					}
				}
				else
				{
					await ctx.RespondAsync(
						$"No Values found for this user. He has to update his hash with ``//setHash <hash>`` first");
					return;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public int[] ParseHash(string hash)
		{
			string newhash = hash.Substring(1);
			if (newhash.Length < 10) return null;

			return this.Decode(Enum.GetNames(typeof(Colors)).Length, newhash);
		}

		[Command("setHash")]
		[Description("Set your own Hash")]
		public async Task SetKinkList(CommandContext ctx, string hash)
		{
			if (!hash.StartsWith(@"``#") && !hash.StartsWith(@"```#"))
			{
				await ctx.RespondAsync("Please pass only the Hash and use codeblocks: `` `` ``\n" +
									   "so ``https://cdn.rawgit.com/Goctionni/KinkList/master/v1.0.2.html#FLTg^zsr=3suNk3IMuiSxnMTaQJBkVb@KAwAdk.lR!neldDd36*Vjm6RX2syjEtp++uVi*LEFO_loYPSaabl5mWB``\n" +
									   "is ``#FLTg^zsr=3suNk3IMuiSxnMTaQJBkVb@KAwAdk.lR!neldDd36*Vjm6RX2syjEtp++uVi*LEFO_loYPSaabl5mWB``");
				return;
			}

			string newHash = hash.Trim('`');

			using var conn = new SqlConnection(Config.DatabaseConnectionString);
			await conn.OpenAsync();

			SqlCommand cmd = new SqlCommand();
			cmd = new SqlCommand($"IF EXISTS (SELECT * FROM KinkHashes WHERE UserID = @UserID)" +
								 $"  UPDATE KinkHashes SET Hash = @Hash " +
								 $"ELSE" +
								 $"  INSERT INTO KinkHashes (UserID, Hash) " +
								 $"VALUES (@UserID, @Hash)",
				conn);

			cmd.Parameters.Add("@UserID", SqlDbType.Decimal).Value = Convert.ToInt64(ctx.Message.Author.Id);
			cmd.Parameters.Add("@Hash", SqlDbType.NText).Value = newHash;

			try
			{
				await cmd.ExecuteNonQueryAsync();
				await ctx.RespondAsync("Saved.");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private int[] Decode(int colorLength, string hash)
		{
			int hashBase = HashChars.Length;
			int outputPow = this.MaxPow(hashBase, MaxSafeNumber);

			var values = new List<int>();
			int numChunks = hash.Length / outputPow;

			for (var i = 0; i < numChunks; i++)
			{
				string chunk = hash.Substring(i * outputPow, outputPow);
				int[] chunkValues = this.DecodeChunk(colorLength, chunk);
				values.AddRange(chunkValues);
			}

			return values.ToArray();
		}

		private int[] DecodeChunk(int colorLength, string chunk)
		{
			int hashBase = HashChars.Length;
			int outputPow = this.MaxPow(hashBase, MaxSafeNumber);
			int inputPow = this.MaxPow(colorLength, Convert.ToInt64(Math.Pow(hashBase, outputPow)));

			Int64 chunkInt = 0;
			for (var i = 0; i < chunk.Length; i++)
			{
				char character = chunk[i];
				int charInt = HashChars.IndexOf(character);
				int pow = chunk.Length - 1 - i;
				double powd = Math.Pow(hashBase, pow) * charInt;
				long intVal = Convert.ToInt64(powd);
				chunkInt += intVal;
			}

			var output = new List<int>();
			for (int pow = inputPow - 1; pow >= 0; pow--)
			{
				double posBase = Math.Floor(Math.Pow(colorLength, pow));
				double posVal = Math.Floor(chunkInt / posBase);
				long subtract = Convert.ToInt64(posBase * posVal);
				output.Add(Convert.ToInt32(posVal));
				chunkInt -= subtract;
			}

			output.Reverse();
			return output.ToArray();
		}

		private int MaxPow(Int64 baseInt, Int64 maxVal)
		{
			var maxPow = 1;
			for (var pow = 1; Math.Pow(baseInt, pow) <= maxVal; pow++) maxPow = pow;
			return maxPow;
		}

		private List<KinkName> ParseKinksText(string kinksText)
		{
			var kinkNames = new List<KinkName>();

			string[] lines = kinksText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

			var cKinkCat = "";
			var cKinkColumns = new string[] { "" };

			foreach (string line in lines)
			{
				if (line.Length == 0)
				{
					continue;
				}

				if (line[0] == '#')
				{
					cKinkCat = line.Substring(1).Trim('\n', ' ', '(', ')');
					continue;
				}

				if (line[0] == '(')
				{
					cKinkColumns = line.Trim('\n', ' ', '(', ')').Split(new[] { ", " }, StringSplitOptions.None);
					continue;
				}

				if (line[0] == '*')
				{
					for (int i = 0; i < cKinkColumns.Length; i++)
					{
						var cKinkName = new KinkName
						{
							Category = cKinkCat,
							Column = cKinkColumns[i],
							Name = line.Trim(' ', '*', '\n'),
							ColumnSide = i
						};
						kinkNames.Add(cKinkName);
					}
				}
			}

			return kinkNames;
		}

		private class KinkName
		{
			public string Category;
			public string Column;
			public int ColumnSide;
			public string Name;
		}
	}
}