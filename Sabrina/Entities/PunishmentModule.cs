using DSharpPlus.Entities;
using Sabrina.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sabrina.Entities
{
	internal abstract class PunishmentModule
	{
		public PunishmentModule(Dictionary<UserSetting.SettingID, UserSetting> settings, List<WheelUserItem> items)
		{
			Settings = settings;
			Items = items;
		}

		public abstract int Chance { get; internal set; }

		public abstract TimeSpan DenialTime { get; internal set; }

		public abstract DiscordEmbed Embed { get; internal set; }

		public virtual IEnumerable<UserSetting.SettingID> RequiredSettings { get; internal set; } = new List<UserSetting.SettingID>();
		public abstract TimeSpan WheelLockTime { get; internal set; }
		internal List<WheelUserItem> Items { get; private set; }
		internal Dictionary<UserSetting.SettingID, UserSetting> Settings { get; private set; }

		public static IEnumerable<PunishmentModule> GetAll(Dictionary<UserSetting.SettingID, UserSetting> settings, List<WheelUserItem> items)
		{
			var allModules = ReflectiveEnumerator.GetEnumerableOfType<PunishmentModule>(settings, items);
			return allModules;
		}

		public async Task ApplySettings(DiscordContext context = null)
		{
			bool dispose = context == null;

			if (context == null)
			{
#pragma warning disable IDE0068 // Use recommended dispose pattern
				context = new DiscordContext();
#pragma warning restore IDE0068 // Use recommended dispose pattern
			}

			var userId = Settings.Values.First().UserId;

			foreach (var setting in Settings)
			{
				await UserSetting.SetSettingAsync(userId, setting.Key, setting.Value.Value, context, false);
			}

			if (dispose)
				context.Dispose();

			await context.SaveChangesAsync();
		}

		public abstract Task Generate();
	}
}