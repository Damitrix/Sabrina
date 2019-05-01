using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Sabrina.Models;
using System;
using System.Threading.Tasks;

namespace Sabrina.Entities.SettingsModules
{
    internal class WheelTaskPreferenceModule : SettingsModule
    {
        public WheelTaskPreferenceModule(DiscordContext context, DiscordDmChannel dm, CommandContext ctx, long userId) : base(context, dm, ctx, userId)
        {
        }

        public override string FriendlyName { get; internal set; } = "Wheel task preference";
        internal override string[] _keys { get; set; } = new[] { "wheel", "task", "preference" };

        internal override async Task<bool> Run()
        {
            WheelExtension.WheelTaskPreferenceSetting? wheelTaskPreference = null;

            var builder = new DiscordEmbedBuilder()
            {
                Title = "What kind of task do you prefer? There are no penalties here.",
                Url = "http://IJustWantThisToBeBlue.com"
            };

            builder.AddField("Edge for 15 Minutes. 30 second Cooldown.", WheelExtension.WheelTaskPreferenceSetting.Time.ToString());
            builder.AddField("Edge 10 times.", WheelExtension.WheelTaskPreferenceSetting.Amount.ToString());
            builder.AddField("Edge 10 times per day, for the next 2 Days.", WheelExtension.WheelTaskPreferenceSetting.Task.ToString());
            builder.AddField("No preference", WheelExtension.WheelTaskPreferenceSetting.Default.ToString());

            await _dm.SendMessageAsync(embed: builder.Build());

            var m = await _ctx.Client.GetInteractivityModule().WaitForMessageAsync(
                x => x.Channel.Id == _dm.Id
                     && x.Author.Id == _ctx.Member.Id,
                TimeSpan.FromSeconds(240));

            if (m == null)
            {
                await _dm.SendMessageAsync($"Or just don't respond at all. That's ok too :(");
                return false;
            }

            if (Enum.TryParse(m.Message.Content, true, out WheelExtension.WheelTaskPreferenceSetting wheelPreferenceSetting))
            {
                wheelTaskPreference = wheelPreferenceSetting;
            }
            else
            {
                await _dm.SendMessageAsync($"Sorry, i didn't get that. You have to precisely enter the name of one of the possible preferences.");
            }

            UserSetting wheelTaskPreferenceSetting = await UserSettingExtension.SetSettingAsync(_userId, UserSettingExtension.SettingID.WheelTaskPreference, (int)wheelTaskPreference.Value, _context);

            return true;
        }
    }
}