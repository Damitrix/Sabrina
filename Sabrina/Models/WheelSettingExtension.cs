using System;
using System.Threading.Tasks;

namespace Sabrina.Models
{
	public static class WheelSettingExtension
	{
		[Flags]
		public enum WheelMode
		{
			Default = 0,
			NoRuin = 1 << 0,
			NoCum = 1 << 1,
			NoDenial = 1 << 2,
			Infinite = NoRuin | NoCum | NoDenial
		}

		public static async Task<WheelMode> GetMode(long guildId, DiscordContext context = null)
		{
			var dispose = context == null;

			if (context == null)
			{
#pragma warning disable IDE0068 // Use recommended dispose pattern
				context = new DiscordContext();
#pragma warning restore IDE0068 // Use recommended dispose pattern
			}

			WheelSetting wheelSetting = await context.WheelSetting.FindAsync(guildId);

			if (dispose)
			{
				context.Dispose();
			}

			return wheelSetting == null ? WheelMode.Default : (WheelMode)wheelSetting.Mode;
		}

		public static Task<WheelMode> GetMode(ulong guildId, DiscordContext context = null)
		{
			return GetMode(Convert.ToInt64(guildId), context);
		}

		public static async Task SetMode(WheelMode newMode, long guildId, DiscordContext context = null, bool save = true)
		{
			var dispose = context == null;

			if (context == null)
			{
#pragma warning disable IDE0068 // Use recommended dispose pattern
				context = new DiscordContext();
#pragma warning restore IDE0068 // Use recommended dispose pattern
			}

			WheelSetting wheelSetting = await context.WheelSetting.FindAsync(guildId);

			if (wheelSetting == null)
			{
				wheelSetting = new WheelSetting()
				{
					GuildId = guildId,
					Mode = (int)newMode
				};

				context.WheelSetting.Add(wheelSetting);
			}
			else
			{
				wheelSetting.Mode = (int)newMode;
			}

			if (save)
			{
				await context.SaveChangesAsync();
			}

			if (dispose)
				context.Dispose();
		}

		public static Task SetMode(WheelMode newMode, ulong guildId, DiscordContext context = null, bool save = true)
		{
			return SetMode(newMode, Convert.ToInt64(guildId), context, save);
		}
	}
}