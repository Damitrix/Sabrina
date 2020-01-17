using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Sabrina.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sabrina.Entities.SettingsModules
{
	internal class ClothingModule : SettingsModule
	{
		public ClothingModule(DiscordContext context, DiscordDmChannel dm, CommandContext ctx, long userId) : base(context, dm, ctx, userId)
		{
		}

		public override string FriendlyName { get; internal set; } = "clothing";
		internal override string[] _keys { get; set; } = new[] { "clothes", "sissy" };

		internal override async Task<bool> Run()
		{
			bool cancel = false;

			while (!cancel)
			{
				var existingClothes = (await WheelItemExtension.GetUserItemsAsync(_userId, _context)).Where(item => item.ItemId > (int)WheelItemExtension.Item.Clothing && item.ItemId < (int)WheelItemExtension.Item.Clothing + 1000).ToList();

				await _dm.SendMessageAsync($"Do you want to [add], or do you want to [remove] some of your clothes? Or are you [done]?");

				var m = await _ctx.Client.GetInteractivity().WaitForMessageAsync(
					   x => x.Channel.Id == _dm.Id
							&& x.Author.Id == _ctx.Member.Id,
					   TimeSpan.FromSeconds(240));

				if (m.TimedOut)
				{
					await _dm.SendMessageAsync($"Or just don't respond at all. That's ok too :(");
					cancel = true;
					return false;
				}

				if (Helpers.RegexHelper.ExitRegex.Match(m.Result.Content).Success)
				{
					cancel = true;
					return true;
				}

				if (Helpers.RegexHelper.AddRegex.Match(m.Result.Content).Success)
				{
					var possibleItems = new List<WheelUserItem>();

					foreach (var item in (WheelItemExtension.Item[])Enum.GetValues(typeof(WheelItemExtension.Item)))
					{
						if ((int)item <= (int)WheelItemExtension.Item.Clothing || (int)item >= (int)WheelItemExtension.Item.Clothing + 1000 || existingClothes.Any(existingItem => existingItem.ItemId == (int)item))
						{
							continue;
						}

						possibleItems.Add(new WheelUserItem()
						{
							ItemId = (int)item,
							UserId = _userId
						});
					}

					var addItems = await new Items.ItemDialog().OpenAsync(possibleItems, Items.ItemDialog.DialogType.Add, _dm, _ctx);

					if (addItems != null && addItems.Any())
					{
						_context.WheelUserItem.AddRange(addItems);
						await _context.SaveChangesAsync();
					}
				}

				if (Helpers.RegexHelper.RemoveRegex.Match(m.Result.Content).Success)
				{
					var removeItems = await new Items.ItemDialog().OpenAsync(existingClothes, Items.ItemDialog.DialogType.Remove, _dm, _ctx);

					if (removeItems != null && removeItems.Any())
					{
						_context.RemoveRange(removeItems);
						await _context.SaveChangesAsync();
					}
				}
			}

			return true;
		}
	}
}