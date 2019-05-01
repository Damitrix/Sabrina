using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Sabrina.Models;
using System;
using System.Threading.Tasks;

namespace Sabrina.Entities.SettingsModules
{
    internal class GenderModule : SettingsModule
    {
        public GenderModule(DiscordContext context, DiscordDmChannel dm, CommandContext ctx, long userId) : base(context, dm, ctx, userId)
        {
        }

        public override string FriendlyName { get; internal set; } = "Gender";
        internal override string[] _keys { get; set; } = new[] { "gender" };

        internal override async Task<bool> Run()
        {
            int? gender = null;

            var builder = new DiscordEmbedBuilder()
            {
                Title = "Please specify the Gender which with you want to be addressed.",
                Url = "https://en.wikipedia.org/wiki/Gender"
            };

            builder.AddField("If you don't want, to set your Gender, use this.", UserSettingExtension.Gender.None.ToString());
            builder.AddField("You have a Penis.", UserSettingExtension.Gender.Male.ToString());
            builder.AddField("You don't have a Penis.", UserSettingExtension.Gender.Female.ToString());

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

            if (Enum.TryParse(m.Message.Content, true, out UserSettingExtension.Gender genderPreference))
            {
                gender = (int)genderPreference;
            }
            else
            {
                await _dm.SendMessageAsync($"Sorry, I didn't get that. You have to precisely enter the name of one of the levels.");
            }

            await UserSettingExtension.SetSettingAsync(_userId, UserSettingExtension.SettingID.Gender, gender, _context);

            return true;
        }
    }
}