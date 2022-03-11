
using Discord;

namespace FatedTimezoneBot.Logic.Dispatcher
{
    public interface IEventDispatcher
    {
        Task FireEvent(string eventName);
        void RegisterEvent(IEventHandler e);
    }
}