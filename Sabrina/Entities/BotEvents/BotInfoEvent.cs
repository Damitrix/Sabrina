using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Sabrina.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sabrina.Entities.BotEvents
{
	internal class BotInfoEvent : BotEvent
	{
		public BotInfoEvent(DiscordClient client) : base(client)
		{
		}

		internal override int EventID { get => 1; }

		public async override Task Run()
		{
			CheckLoaded();

			using var context = new DiscordContext();

			foreach (var location in context.EventLocation.Where(el => el.EventId == DBEvent.Id).ToArray())
			{
				var latestRun = context.EventRun.Where(er => er.ChannelId == location.ChannelId).OrderByDescending(er => er.Time).FirstOrDefault();

				if (latestRun == null || (latestRun != null && DateTime.Now - latestRun.Time > DBEvent.TriggerTimeSpan))
				{
					DiscordChannel channel = null;

					try
					{
						channel = await Client.GetChannelAsync(Convert.ToUInt64(location.ChannelId));
					}
					catch (UnauthorizedException)
					{
						context.EventRun.Add(new EventRun() { ChannelId = location.ChannelId, EventId = EventID, Time = DateTime.Now });
						continue;
					}

					DiscordEmbedBuilder builder = new DiscordEmbedBuilder
					{
						Author = new DiscordEmbedBuilder.EmbedAuthor
						{
							IconUrl = Client.CurrentUser.AvatarUrl,
							Name = Client.CurrentUser.Username
						},
						Color = DiscordColor.VeryDarkGray,
						Description =
							"Hey Guys and Gals, i'm Sabrina. Since Mistress can't tend to every single one of your pathetic little needs, i'm here to help her out." +
							Environment.NewLine +
							"I've got a bunch of neat little Commands to ~~torture~~ help you. You'll probably only ever need 3 though." +
							Environment.NewLine + Environment.NewLine +
							"``//orgasmwheel``" + Environment.NewLine +
							"Use this, to spin the \"Wheel of Misfortune\". It contains fun little Tasks and \"Rewards\", that Mistress Aki herself has created. " +
							"(That means, if you're unhappy with your outcome, you know where to complain.... if you dare to.)" + Environment.NewLine +
							"You can also use ``//fullorgasmwheel`` if you want to get all Tasks at once." +
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

					context.EventRun.Add(new EventRun() { ChannelId = location.ChannelId, EventId = EventID, Time = DateTime.Now });
				}
			}

			await context.SaveChangesAsync();
		}
	}
}