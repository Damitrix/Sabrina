using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sabrina.Models
{
	public static class UserSettingExtensions
	{
		/// <summary>
		/// Gets the value of a Setting
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="setting"></param>
		/// <returns>
		/// Enum value of setting. Default of enum if no default is set, otherwise custom default.
		/// </returns>
		public static T GetValue<T>(this UserSetting setting)
		{
			if (setting == null || setting.Value == null)
			{
				if (UserSetting.Defaults.ContainsKey(typeof(T)))
				{
					return (T)(UserSetting.Defaults[typeof(T)] as object);
				}
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
	}

	public partial class UserSetting
	{
		public static Dictionary<Type, Enum> Defaults = new Dictionary<Type, Enum>()
		{
			{ typeof(AnalLevel), AnalLevel.None},
			{ typeof(BondageLevel), BondageLevel.None},
			{ typeof(CBTLevel), CBTLevel.None},
			{ typeof(DungeonDifficulty), DungeonDifficulty.Normal},
			{ typeof(Gender), Gender.None},
			{ typeof(PublicLevel), PublicLevel.None},
			{ typeof(SissyLevel), SissyLevel.None},
			{ typeof(WheelDifficultyPreference), WheelDifficultyPreference.Default},
			{ typeof(WheelTaskPreferenceSetting), WheelTaskPreferenceSetting.Default}
		};

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

		[Flags]
		public enum Outcome
		{
			NotSet = 1,
			Denial = 2,
			Ruin = 4,
			Orgasm = 8,
			Edge = 16,
			Task = 32,
			All = ~0
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

		public enum WheelDifficultyPreference
		{
			Baby = 1,
			Easy,
			Default,
			Hard,
			Masterbater
		}

		[Flags]
		public enum WheelTaskPreferenceSetting
		{
			Default,
			Task,
			Time,
			Amount
		}

		public static IQueryable<Models.UserSetting> GetAllSettings(ulong userId, DiscordContext context)
		{
			return GetAllSettings(Convert.ToInt64(userId), context);
		}

		public static IQueryable<Models.UserSetting> GetAllSettings(long userId, DiscordContext context)
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