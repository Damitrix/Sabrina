using DSharpPlus;
using Sabrina.Entities.BotEvents;
using Sabrina.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Sabrina.Bots
{
	internal class EventBot : IDisposable
	{
		private readonly CancellationToken _cancellationToken;
		private readonly Dictionary<BotEvent, Timer> BotEvents = new Dictionary<BotEvent, Timer>();
		private readonly DiscordClient Client;
		private Timer _botInfoTimer;
		private bool disposed = false;

		public EventBot(DiscordClient client, CancellationToken token)
		{
			Client = client;
			_cancellationToken = token;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public async Task Start()
		{
			// Run in background
			await Task.Run(async () =>
			{
				var context = new DiscordContext();

				var botInfoEvent = new BotInfoEvent(Client);
				await botInfoEvent.Load(context);
				var botInfoNextRun = await botInfoEvent.GetNextRun();

				botInfoNextRun = botInfoNextRun == TimeSpan.Zero ? TimeSpan.FromSeconds(10) : botInfoNextRun;

				_botInfoTimer = new Timer(botInfoNextRun.TotalMilliseconds) { AutoReset = false };
				_botInfoTimer.Elapsed += async (object sender, ElapsedEventArgs e) => await RunEvent(botInfoEvent);
				BotEvents.Add(botInfoEvent, _botInfoTimer);

				_botInfoTimer.Start();
			}, _cancellationToken);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing)
			{
				_botInfoTimer.Stop();
				_botInfoTimer.Dispose();
			}

			disposed = true;
		}

		private async Task RunEvent(BotEvent botEvent)
		{
			await botEvent.Run();

			var nextTrigger = await botEvent.GetNextRun();

			BotEvents[botEvent].Interval = nextTrigger.TotalMilliseconds;
			BotEvents[botEvent].Start();
		}
	}
}