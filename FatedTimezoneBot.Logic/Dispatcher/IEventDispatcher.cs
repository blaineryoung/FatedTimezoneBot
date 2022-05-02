
using Discord;

namespace FatedTimezoneBot.Logic.Dispatcher
{
    public interface IEventDispatcher
    {
        Task FireEvent(string eventName);
        Task RegisterEvent(IEventHandler e);
    }
}