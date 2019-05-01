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
    internal class BondageGearModule : SettingsModule
    {
        public BondageGearModule(DiscordContext context, DiscordDmChannel dm, CommandContext ctx, long userId) : base(context, dm, ctx, userId)
        {
        }

        public override string FriendlyName { get; internal set; } = "bondage gear";
        internal override string[] _keys { get; set; } = new[] { "bondage", "gear", "toys" };

        internal override async Task<bool> Run()
        {
            // See if User has bondage level set beforehand
            if (await UserSettingExtension.GetSettingAsync(_userId, UserSettingExtension.SettingID.BondageLevel, _context) == null)
            {
                await _dm.TriggerTypingAsync();
                await Task.Delay(1000);
                await _dm.SendMessageAsync($"First i've got to know something else.");

                var setBondageLevel = await new BondageLevelModule(_context, _dm, _ctx, _userId).Run();

                if (setBondageLevel == false)
                {
                    return false;
                }

                var bondageLevel = await UserSettingExtension.GetSettingAsync(_userId, UserSettingExtension.SettingID.BondageLevel, _context);

                if (bondageLevel.GetValue<UserSettingExtension.BondageLevel>() == UserSettingExtension.BondageLevel.None)
                {
                    await _dm.TriggerTypingAsync();
                    await Task.Delay(1000);
                    await _dm.SendMessageAsync($"So you don't wanna do any Bondage? ...ok");
                    return true;
                }
            }

            bool cancel = false;

            while (!cancel)
            {
                var existingBondageGear = (await WheelItemExtension.GetUserItemsAsync(_userId, _context)).Where(item => item.ItemId > (int)WheelItemExtension.Item.Bondage && item.ItemId < (int)WheelItemExtension.Item.Bondage + 1000).ToList();

                await _dm.SendMessageAsync($"Do you want to [add], or do you want to [remove] some of your bondage items? Or are you [done]?");

                var m = await _ctx.Client.GetInteractivityModule().WaitForMessageAsync(
                       x => x.Channel.Id == _dm.Id
                            && x.Author.Id == _ctx.Member.Id,
                       TimeSpan.FromSeconds(240));

                if (m == null)
                {
                    await _dm.SendMessageAsync($"Or just don't respond at all. That's ok too :(");
                    cancel = true;
                    return false;
                }

                if (Helpers.RegexHelper.ExitRegex.Match(m.Message.Content).Success)
                {
                    cancel = true;
                    return true;
                }

                if (Helpers.RegexHelper.AddRegex.Match(m.Message.Content).Success)
                {
                    var possibleItems = new List<WheelUserItem>();

                    foreach (var item in (WheelItemExtension.Item[])Enum.GetValues(typeof(WheelItemExtension.Item)))
                    {
                        if ((int)item <= (int)WheelItemExtension.Item.Bondage || (int)item >= (int)WheelItemExtension.Item.Bondage + 1000 || existingBondageGear.Any(gear => gear.ItemId == (int)item))
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

                if (Helpers.RegexHelper.RemoveRegex.Match(m.Message.Content).Success)
                {
                    var removeItems = await new Items.ItemDialog().OpenAsync(existingBondageGear, Items.ItemDialog.DialogType.Remove, _dm, _ctx);

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