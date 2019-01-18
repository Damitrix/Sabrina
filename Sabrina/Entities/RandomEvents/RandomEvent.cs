using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sabrina.Entities.RandomEvents
{
    public abstract class RandomEvent
    {
        protected readonly TimeSpan MinTimeBetweenCalls;

        protected readonly TimeSpan MaxTimeBetweenCalls;

        protected readonly DiscordClient _client;

        public RandomEvent(DiscordClient client)
        {
            _client = client;
        }

        public abstract void Run();
    }
}
