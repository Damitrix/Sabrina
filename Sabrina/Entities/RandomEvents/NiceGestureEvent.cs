using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sabrina.Entities.RandomEvents
{
    class NiceGestureEvent : RandomEvent
    {
        private new readonly TimeSpan MinTimeBetweenCalls = new TimeSpan(16, 0, 0);
        private new readonly TimeSpan MaxTimeBetweenCalls = new TimeSpan(48, 0, 0);

        public NiceGestureEvent(DiscordClient client) : base(client)
        {
            
        }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }
}
