using Discord;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Dispatcher
{
    public class EventDispatcher : IEventDispatcher
    {
        IDiscordClient _client;

        private Dictionary<string, EventInfo> _events = new Dictionary<string, EventInfo>();
        private ILogger _logger;

        public EventDispatcher(IDiscordClient client, ILogger logger)
        {
            this._client = client;
            this._logger = logger;
        }

        public async Task RegisterEvent(IEventHandler e)
        {
            if (_events.ContainsKey(e.Name))
            {
                return;
            }

            if (e.RunAtStart)
            {
                // Don't wait since it takes a while.
                e.HandleEvent().ConfigureAwait(false);
            }

            System.Timers.Timer t = new System.Timers.Timer(e.Interval);
            t.Elapsed += async (sender, a) => await e.HandleEvent();
            t.AutoReset = true;
            t.Enabled = true;
            t.Start();

            EventInfo info = new EventInfo(e.Name, t, e);
            _events.Add(e.Name, info);

            t.Start();
        }

        public async Task FireEvent(string eventName)
        {
            IEnumerable<IEventHandler> events = _events.Where(x => x.Value.Name.Equals(eventName, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value.Handler);
            foreach (IEventHandler e in events)
            {
                _logger.Information("Executing event {eventname}", e.Name);
                await e.HandleEvent();
            }
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
