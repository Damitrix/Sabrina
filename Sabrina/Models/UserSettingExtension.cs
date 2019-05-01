using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sabrina.Models
{
    public static class UserSettingExtension
    {
        public enum AnalLevel
        {
            None = 0,
            Light = 1,
            Normal = 2,
            Hardcore = 3
        }

        public enum BanType
        {
            None,
            NoHentai,
            NoRegularPorn,
            OnlyHentai,
            OnlyRegularPorn,
            OnlyTrapPorn
        }

        public enum BondageLevel
        {
            None = 0,
            Light = 1,
            Normal = 2,
            Hardcore = 3
        }

        public enum CBTLevel
        {
            None = 0,
            Light = 1,
            Normal = 2,
            Hardcore = 3
        }

        public enum DenialReason
        {
            None,
            Cooldown
        }

        public enum DungeonDifficulty
        {
            Nonexistant,
            Easy,
            Beginner,
            Normal,
            NormalPlus,
            Hard,
            Harder,
            Extreme
        }

        public enum Gender
        {
            None = 0,
            Male = 1,
            Female = 2
        }

        [Flags]
        public enum LockReason
        {
            None = 1,
            Cooldown = 2,
            Task = 4,
            Extension = 8
        }

        public enum PublicLevel
        {
            None = 0,
            Light = 1,
            Normal = 2,
            Hardcore = 3
        }

        public enum SettingID
        {
            WheelDifficulty = 1,
            WheelTaskPreference = 2,
            BondageLevel = 3,
            Toys = 4,
            CBTLevel = 5,
            DungeonDifficulty,
            AnalLevel,
            PeeLevel,
            SissyLevel,
            TrapLevel = 9,
            DegrationLevel,
            PublicLevel,
            BanType,
            BanEnd,
            Gender
        }

        public enum SissyLevel
        {
            None = 0,
            Light = 1,
            Normal = 2,
            Hardcore = 3
        }

        public static IQueryable<UserSetting> GetAllSettings(ulong userId, DiscordContext context)
        {
            return GetAllSettings(Convert.ToInt64(userId), context);
        }

        public static IQueryable<UserSetting> GetAllSettings(long userId, DiscordContext context)
        {
            return context.UserSetting.Where(setting => setting.UserId == userId);
        }

        public static T GetSettingAsType<T>(string value) where T : Enum
        {
            if (value == null)
            {
                return default;
            }

            if (int.TryParse(value, out int parsed))
            {
                return (T)Enum.ToObject(typeof(T), parsed);
            }

            return default;
        }

        public static async Task<UserSetting> GetSettingAsync(long userId, SettingID name, DiscordContext context)
        {
            return await context.UserSetting.Where(setting => setting.UserId == userId).FirstOrDefaultAsync(setting => setting.SettingId == (int)name);
        }

        public static T GetValue<T>(this UserSetting setting)
        {
            if (setting == null || setting.Value == null)
            {
                return default;
            }

            if (typeof(T).IsEnum)
            {
                if (int.TryParse(setting.Value, out int parsed))
                {
                    return (T)Enum.ToObject(typeof(T), parsed);
                }
            }

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.DateTime:
                    if (DateTime.TryParse(setting.Value, out DateTime parsed))
                    {
                        return (T)(parsed as object);
                    }
                    break;
            }

            return default;
        }

        public static async Task<UserSetting> SetSettingAsync(long userId, SettingID name, object value, DiscordContext context, bool save = true)
        {
            var userSetting = await context.UserSetting.Where(setting => setting.UserId == userId).FirstOrDefaultAsync(setting => setting.SettingId == (int)name);

            string valueString = "";

            Type valueType = value.GetType();

            if (valueType.IsEnum)
            {
                valueString = ((int)value).ToString();
            }
            else
            {
                valueString = value.ToString();
            }

            if (userSetting != null)
            {
                userSetting.Value = valueString;
            }
            else
            {
                userSetting = new UserSetting()
                {
                    SettingId = (int)name,
                    UserId = userId,
                    Value = valueString
                };

                await context.UserSetting.AddAsync(userSetting);
            }

            if (save)
            {
                await context.SaveChangesAsync();
            }

            return userSetting;
        }
    }
}