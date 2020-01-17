using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Sabrina.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sabrina.Entities.BotEvents
{
	public abstract class BotEvent
	{
		internal bool _IsLoaded = false;

		public BotEvent(DiscordClient client)
		{
			Client = client;
		}

		public virtual Event DBEvent { get; private set; }
		internal DiscordClient Client { private set; get; }
		internal abstract int EventID { get; }

		public async Task<TimeSpan> GetNextRun(DiscordContext context = null)
		{
			bool dispose = context == null;

			CheckLoaded();

			if (context == null)
			{
#pragma warning disable IDE0068 // Use recommended dispose pattern
				context = new DiscordContext();
#pragma warning restore IDE0068 // Use recommended dispose pattern
			}

			var lastEventRun = await context.EventRun.OrderByDescending(er => er.Time).FirstOrDefaultAsync(er => er.EventId == DBEvent.Id);

			if (lastEventRun == null)
			{
				return TimeSpan.FromSeconds(1);
			}

			var lastRun = lastEventRun.Time;

			var timeDifference = DateTime.Now - lastRun;

			if (timeDifference > DBEvent.TriggerTimeSpan)
			{
				return TimeSpan.FromSeconds(1);
			}

			if (dispose)
				context.Dispose();

			return DBEvent.TriggerTimeSpan - timeDifference;
		}

		public async Task Load(DiscordContext context)
		{
			DBEvent = await context.Event.FindAsync(EventID);
			_IsLoaded = true;
		}

		public Task Refresh(DiscordContext context) => Load(context);

		public virtual Task Run()
		{
			CheckLoaded();

			return Task.CompletedTask;
		}

		internal void CheckLoaded()
		{
			if (!_IsLoaded)
			{
				throw new InvalidOperationException("Event has not been loaded");
			}

			if (DBEvent == null)
			{
				throw new InvalidOperationException("Database Information has not been loaded correctly");
			}
		}
	}
}