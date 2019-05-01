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
        public PunishmentModule(Dictionary<UserSettingExtension.SettingID, UserSetting> settings, List<WheelUserItem> items)
        {
            _settings = settings;
            _items = items;
        }

        public abstract int Chance { get; internal set; }

        public abstract TimeSpan DenialTime { get; internal set; }

        public abstract DiscordEmbed Embed { get; internal set; }

        public abstract IEnumerable<UserSettingExtension.SettingID> RequiredSettings { get; internal set; }
        public abstract TimeSpan WheelLockTime { get; internal set; }
        internal List<WheelUserItem> _items { get; private set; }
        internal Dictionary<UserSettingExtension.SettingID, UserSetting> _settings { get; private set; }

        public static IEnumerable<PunishmentModule> GetAll(Dictionary<UserSettingExtension.SettingID, UserSetting> settings, List<WheelUserItem> items, DiscordContext context = null)
        {
            if (context == null)
            {
                context = new DiscordContext();
            }

            var allModules = ReflectiveEnumerator.GetEnumerableOfType<PunishmentModule>(settings, items);
            return allModules;
        }

        public async Task ApplySettings(DiscordContext context = null)
        {
            if (context == null)
            {
                context = new DiscordContext();
            }

            var userId = _settings.Values.First().UserId;

            foreach (var setting in _settings)
            {
                await UserSettingExtension.SetSettingAsync(userId, setting.Key, setting.Value.Value, context, false);
            }

            await context.SaveChangesAsync();
        }

        public abstract Task Generate();
    }
}