using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sabrina.Models
{
    public static class WheelItemExtension
    {
        /// <summary>
        /// All possible Catagories, their Items and their Variations a user can have.
        /// </summary>
        /// Each Category has a Range of 1000
        public enum Item
        {
            // Bondage
            Bondage = 0,

            Blindfold = 10,
            Rope = 20,
            String = 30,
            Cuffs = 40,
            ChastityDevice = 50,
            Clamps = 60,
            Gag = 70,

            // Toys
            Toy = 1000,

            AnalPlug = 1010,
            Vibrator = 1020,
            Dildo = 1030,

            // Clothing
            Clothing = 2000,

            Panties = 2010,
            Socks = 2020,
            Kneesocks = 2021,
            Thighhighs = 2022,
            Collar = 2030,
            AnimalEars = 2040,
            CatEars = 2041,
            DogEars = 2042,
            Bra = 2050
        }

        public static async Task AddItemToUser(long userId, WheelUserItem item, DiscordContext context = null)
        {
            if (context == null)
            {
                context = new DiscordContext();
            }

            if (await context.WheelUserItem.AnyAsync(cItem => cItem.UserId == userId && cItem.ItemId == item.Id))
            {
                return;
            }

            var userItem = new WheelUserItem()
            {
                UserId = userId,
                ItemId = item.Id
            };

            context.WheelUserItem.Add(userItem);

            await context.SaveChangesAsync();
        }

        public static async Task AddItemToUser(ulong userId, WheelUserItem item, DiscordContext context = null)
        {
            await AddItemToUser(Convert.ToInt64(userId), item, context);
        }

        public static Item GetItemCategory(Item item)
        {
            return (Item)(((int)((float)item / 1000)) * 1000);
        }

        public static Item GetItemSubCategory(Item item)
        {
            return (Item)(((int)((float)item / 10)) * 10);
        }

        public static async Task<IEnumerable<WheelUserItem>> GetUserItemsAsync(ulong userId, DiscordContext context = null)
        {
            return await GetUserItemsAsync(Convert.ToInt64(userId), context);
        }

        public static async Task<IEnumerable<WheelUserItem>> GetUserItemsAsync(long userId, DiscordContext context = null)
        {
            if (context == null)
            {
                context = new DiscordContext();
            }

            var items = context.WheelUserItem.Where(item => item.UserId == userId);

            return await items.ToListAsync();
        }

        public static async Task RemoveItemFromUser(ulong userId, WheelUserItem item, DiscordContext context = null)
        {
            await RemoveItemFromUserAsync(Convert.ToInt64(userId), item, context);
        }

        public static async Task RemoveItemFromUserAsync(long userId, WheelUserItem item, DiscordContext context = null)
        {
            if (!await context.WheelUserItem.AnyAsync(cItem => cItem.UserId == userId && cItem.ItemId == item.Id))
            {
                return;
            }

            context.WheelUserItem.Remove(await context.WheelUserItem.SingleAsync(uItem => uItem.ItemId == item.Id && uItem.UserId == userId));

            await context.SaveChangesAsync();
        }
    }
}