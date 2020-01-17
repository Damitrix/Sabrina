using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Sabrina.Entities;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sabrina.Commands
{
	public class ExternalAPI : BaseCommandModule
	{
		[Command("urbandictionary"), Aliases("urban")]
		[Description("Search for something on urban dictionary")]
		public async Task UrbanDictionary(CommandContext ctx, [Description("The word(s) to search for on Urban Dictionary")] params string[] searchText)
		{
			var joinedSearch = "term=" + String.Join(" ", searchText);

			var uriBuilder = new UriBuilder("http://api.urbandictionary.com/v0/define");
			var parameters = System.Web.HttpUtility.ParseQueryString(joinedSearch);
			uriBuilder.Query = parameters.ToString();

			string json;

			using (var client = new HttpClient())
			{
				json = await client.GetStringAsync(uriBuilder.ToString());
			}

			var urbanResponse = Urban.FromJson(json);

			DiscordEmbedBuilder builder;

			if (urbanResponse.List.Count == 0)
			{
				builder = new DiscordEmbedBuilder()
				{
					Author = new DiscordEmbedBuilder.EmbedAuthor()
					{
						Name = "Urban Dictionary",
						Url = "https://www.urbandictionary.com/"
					},
					Title = $"Urban Definition of \"{joinedSearch}\" not found"
				};
			}
			else
			{
				var urbanDefinition = urbanResponse.List[0];

				var uriBuilderSite = new UriBuilder("http://api.urbandictionary.com/v0/define");
				parameters = System.Web.HttpUtility.ParseQueryString(joinedSearch);
				uriBuilder.Query = parameters.ToString();

				builder = new DiscordEmbedBuilder()
				{
					Author = new DiscordEmbedBuilder.EmbedAuthor()
					{
						Name = "Urban Dictionary",
						Url = "https://www.urbandictionary.com/"
					},
					Title = $"Urban Definition of \"{urbanDefinition.Word}\"",
					Description = urbanDefinition.Definition,
					Color = DiscordColor.MidnightBlue,
					Url = uriBuilderSite.ToString()
				};
			}
			await ctx.RespondAsync(embed: builder.Build());
		}
	}
}