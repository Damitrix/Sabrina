using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Sabrina.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sabrina.Entities.Items
{
	internal class ItemDialog
	{
		public enum DialogType
		{
			Add,
			Remove
		}

		public async Task<IEnumerable<WheelUserItem>> OpenAsync(List<WheelUserItem> items, DialogType dialogType, DiscordChannel channel, CommandContext ctx)
		{
			List<Page> pages = new List<Page>();

			int index = 0;
			while (index < items.Count())
			{
				var builder = new DiscordEmbedBuilder()
				{
					Title = $"{(dialogType == DialogType.Add ? "possible" : "owned")} Items",
					Description = $"These are your {(dialogType == DialogType.Add ? "possible" : "owned")} Items"
				};

				var cItems = items.Skip(index).Take(5);

				foreach (var item in cItems)
				{
					builder.AddField(((WheelItemExtension.Item)item.ItemId).ToFormattedText(), item.ItemId.ToString());
				}

				index += cItems.Count();

				var page = new Page(embed: builder);

				pages.Add(page);
			}

			new Thread(async () =>
			{
				try
				{
					await ctx.Client.GetInteractivity().SendPaginatedMessageAsync(channel, ctx.User, pages, timeoutoverride: TimeSpan.FromMinutes(3), behaviour: DSharpPlus.Interactivity.Enums.PaginationBehaviour.Ignore);
				}
				catch (Exception)
				{
				}
			}).Start();

			await channel.SendMessageAsync($"Please give me either the Names or the Numbers of the Items you want to {(dialogType == DialogType.Add ? "add" : "remove")}. Type \"done\" when you are done.");

			var addOrRemoveItems = new List<WheelUserItem>();
			bool cancel = false;

			// Ask for more until user cancels or timeout
			while (!cancel)
			{
				var m = await ctx.Client.GetInteractivity().WaitForMessageAsync(
				   x => x.Channel.Id == channel.Id
						&& x.Author.Id == ctx.Member.Id,
				   TimeSpan.FromSeconds(240));

				if (m.TimedOut)
				{
					await channel.SendMessageAsync($"Or just don't respond at all. That's ok too :(");
					return null;
				}

				if (Helpers.RegexHelper.ExitRegex.Match(m.Result.Content).Success)
				{
					cancel = true;
					break;
				}

				foreach (var text in m.Result.Content.Split(new[] { Environment.NewLine, ",", ";" }, StringSplitOptions.RemoveEmptyEntries))
				{
					if (string.IsNullOrWhiteSpace(text))
					{
						continue;
					}

					WheelItemExtension.Item? itemToAddOrRemove = null;

					if (Enum.TryParse(text.Replace(" ", ""), true, out WheelItemExtension.Item item))
					{
						itemToAddOrRemove = item;
					}
					else if (Int32.TryParse(text, out int itemNumber))
					{
						if (Enum.IsDefined(typeof(WheelItemExtension.Item), itemNumber))
						{
							itemToAddOrRemove = (WheelItemExtension.Item)itemNumber;
						}
					}
					else
					{
						await channel.SendMessageAsync($"Sorry, i could not understand the meaning of {text}");
						continue;
					}

					if (itemToAddOrRemove != null)
					{
						addOrRemoveItems.AddRange(items.Where(i => i.ItemId == (int)itemToAddOrRemove));
					}
				}
			}

			return (addOrRemoveItems as IEnumerable<WheelUserItem>).DistinctBy(item => item.ItemId);
		}
	}
}