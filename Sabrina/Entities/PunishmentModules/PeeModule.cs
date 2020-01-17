using DSharpPlus.Entities;
using Sabrina.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sabrina.Entities.PunishmentModules
{
    internal class PeeModule : PunishmentModule
    {
        public PeeModule(Dictionary<UserSetting.SettingID, UserSetting> settings, List<WheelUserItem> items) : base(settings, items)
        {
        }

        public override int Chance { get; internal set; } = 0;
        public override TimeSpan DenialTime { get; internal set; }
        public override DiscordEmbed Embed { get; internal set; }
        public override TimeSpan WheelLockTime { get; internal set; }

        public override Task Generate()
        {
            return Task.CompletedTask;
        }
    }
}