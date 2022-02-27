using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Dispatcher
{
    public class EventDispatcher
    {
        IDiscordClient _client;

        private Dictionary<string, EventInfo> _events = new Dictionary<string, EventInfo>();

        public EventDispatcher(IDiscordClient client)
        {
            this._client = client;
        }

        public void RegisterEvent(IEventHandler e)
        {
            if (_events.ContainsKey(e.Name))
            {
                return;
            }

            System.Timers.Timer t = new System.Timers.Timer(e.Interval);
            t.Elapsed += async (sender, a) => await e.HandleEvent();

            EventInfo info = new EventInfo(e.Name, t, e);
            _events.Add(e.Name, info);

            t.Start();
        }
    }

    internal class EventInfo
    {
        internal string Name { get; private set; }

        internal System.Timers.Timer Timer { get; private set; }

        internal IEventHandler Handler { get; private set; }

        public EventInfo(string name, System.Timers.Timer timer, IEventHandler handler)
        {
            this.Name = name;
            this.Timer = timer;
            this.Handler = handler;
        }
    }
}
