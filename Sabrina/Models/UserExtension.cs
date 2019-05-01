using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sabrina.Models
{
    public static class UserExtension
    {
        private static object __lockObj = new object();

        public static async Task<Users> GetUser(long userId, DiscordContext context = null)
        {
            if (context == null)
            {
                context = new DiscordContext();
            }

            var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                lock (__lockObj)
                {
                    lock (context)
                    {
                        if (!context.Users.Any(u => u.UserId == userId))
                        {
                            user = new Users()
                            {
                                DenialTime = DateTime.Now,
                                BanTime = DateTime.Now,
                                LockTime = DateTime.Now,
                                MuteTime = DateTime.Now,
                                SpecialTime = DateTime.Now,
                                UserId = userId
                            };

                            context.Users.Add(user);
                            context.SaveChangesAsync().GetAwaiter().GetResult();
                        }
                        else
                        {
                            user = context.Users.FindAsync(userId).GetAwaiter().GetResult();
                        }
                    }
                }
            }

            return user;
        }

        public static async Task<Users> GetUser(ulong userId, DiscordContext context = null)
        {
            return await GetUser(Convert.ToInt64(userId), context);
        }
    }
}