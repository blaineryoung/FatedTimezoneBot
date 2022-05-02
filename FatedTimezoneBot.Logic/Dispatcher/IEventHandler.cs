using FatedTimezoneBot.Logic.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Dispatcher
{
    public interface IEventHandler
    {
        Task HandleEvent();

        string Name { get; }

        ulong Interval { get; }

        bool RunAtStart { get; }
    }
}
