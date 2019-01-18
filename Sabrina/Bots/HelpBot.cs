using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Configuration;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Sabrina.Models;

namespace Sabrina.Bots
{
    internal class HelpBot
    {
        private readonly DiscordClient _client;

        private Timer _helpPostTimer;

        public HelpBot(DiscordClient client)
        {
            _client = client;

            //SendPatreonUpdateOnce().GetAwaiter().GetResult();

            _helpPostTimer = new Timer(TimeSpan.FromMinutes(30).TotalMilliseconds)
            {
                AutoReset = true
            };
            _helpPostTimer.Elapsed += async (object sender, ElapsedEventArgs e) => await OnTimerElapse();
            _helpPostTimer.Start();

            Task.Run(async () => await OnTimerElapse());
        }

        private async Task SendPatreonUpdateOnce()
        {
            var channel = _client.GetChannelAsync(448781033278799882);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    IconUrl = "https://cdn.discordapp.com/avatars/335437183127257089/a_1a4467f73d1b56fcbf996b4bb9d8b663.gif?size=2048",
                    Name = "Mistress Aki / YourAnimeAddiction",
                    Url = ""
                },
                Color = DiscordColor.Gold,
                Description = @"Closing Patreon

                I'm sure many of you are aware of the Patreon debacle going on right now. I'm not about to get political on a hentai community Patreon page,
                but suffice it to say that I do not support censorship in any form, especially targeted censorship, and will be unpublishing this page on JANUARY 1st 2019.I truly want to thank everyone who has been a patron and supported me in ways I could never have imagined. Thank you so much<3
                That being said, let's make some points clear:

                - Patreon - exclusive content(videos and pictures) will continue to be released publicly, they will not vanish forever.
                -I do not have a Patreon replacement since just about every service does not allow adult /explicit content.
                -I plan to still create new, free, public content in the future.
                -I will return to Discord soon (currently lost my 2FA phone and can't login to Discord >.<).

                Thanks again everyone and I hope you have a wonderful New Year! <3",
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    IconUrl = @"https://c5.patreon.com/external/logo/downloads_logomark_color_on_white@2x.png",
                    Text = "Patreon Post"
                }
            };

            await _client.SendMessageAsync(await channel,embed: builder.Build());
        }

        private async Task OnTimerElapse()
        {
            var context = new DiscordContext();
            var now = DateTime.Now;

            foreach (var setting in context.SabrinaSettings)
            {
                if (setting.WheelChannel != null && setting.LastWheelHelpPost == null || setting.LastWheelHelpPost < now - TimeSpan.FromDays(1))
                {
                    var channel = await _client.GetChannelAsync(Convert.ToUInt64(setting.WheelChannel.Value));

                    DiscordEmbedBuilder builder = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            IconUrl =
                        "https://cdn.discordapp.com/avatars/450771319479599114/d7dd3e2ec296542f170e84c264de78ce.png",
                            Name = "Sabrina"
                        },
                        Color = DiscordColor.VeryDarkGray,
                        Description =
                            "Hey Guys and Gals, i'm Sabrina. Since Mistress can't tend to every single one of your pathetic little needs, i'm here to help her out." +
                            Environment.NewLine +
                            "I've got a bunch of neat little Commands to ~~torture~~ help you. You'll probably only ever need 3 though." +
                            Environment.NewLine + Environment.NewLine +
                            "``//orgasmwheel``" + Environment.NewLine +
                            "Use this, to spin the \"Wheel of Misfortune\". It contains fun little Tasks and \"Rewards\", that Mistress Aki herself has created. " +
                            "(That means, if you're unhappy with your outcome, you know where to complain.... if you dare to.)" +
                            Environment.NewLine + Environment.NewLine +
                            "``//denialtime``" + Environment.NewLine +
                            "This will show you, when exactly " + Environment.NewLine +
                            "    a) You are able to spin again" + Environment.NewLine +
                            "    b) You are not denied anymore" + Environment.NewLine +
                            "Which means, that you may spin the wheel while denied. But that also means, that you can also not be denied, while being excluded from the wheel." +
                            Environment.NewLine + Environment.NewLine +
                            "``//settings setup``" +
                            Environment.NewLine +
                            "When you issue this command, i will assist you with setting up the difficulty of the wheel and other stuffs. Just wait for my dm.",
                        Title = "Introduction",
                        Footer = new DiscordEmbedBuilder.EmbedFooter()
                        {
                            IconUrl = "https://cdn.discordapp.com/avatars/249216025931939841/a_94cf2ac609424257706d6a611f5dd7aa.gif",
                            Text = "If something doesn't seem right, please complain to Salem :)"
                        }
                    };

                    await channel.SendMessageAsync(embed: builder.Build());

                    setting.LastWheelHelpPost = DateTime.Now;
                }

                if (setting.FeetChannel != null && setting.LastDeepLearningPost == null || setting.LastDeepLearningPost < now - TimeSpan.FromDays(1))
                {
                    var channel = await _client.GetChannelAsync(Convert.ToUInt64(setting.FeetChannel.Value));

                    DiscordEmbedBuilder builder = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            IconUrl =
                        "https://cdn.discordapp.com/avatars/450771319479599114/d7dd3e2ec296542f170e84c264de78ce.png",
                            Name = "Sabrina"
                        },
                        Color = DiscordColor.VeryDarkGray,
                        Description =
                            "Heyo, it's me! Your beloved Sabrina!" + Environment.NewLine +
                            "For the slow ones of you: I'm posting a neat little Picture here every now and then." + Environment.NewLine + Environment.NewLine +
                            "Since i can't read minds, i'd love for you, to upvote the pictures you like." + Environment.NewLine +
                            "Why?" + Environment.NewLine +
                            "Well, smartypants, if i feel confident enough about your preferences, i'll start posting pictures specifically chosen for you! <3"
                            + Environment.NewLine + Environment.NewLine
                            //+ "You can also get your fix a little earlier, by using the command ``//boostDL``"
                            ,
                        Title = "Introduction",
                        Footer = new DiscordEmbedBuilder.EmbedFooter()
                        {
                            IconUrl = "https://cdn.discordapp.com/avatars/249216025931939841/a_94cf2ac609424257706d6a611f5dd7aa.gif",
                            Text = "If something doesn't seem right, please complain to Salem :)"
                        }
                    };

                    await channel.SendMessageAsync(embed: builder.Build());

                    setting.LastDeepLearningPost = DateTime.Now;
                }
            }

            await context.SaveChangesAsync();
        }
    }
}