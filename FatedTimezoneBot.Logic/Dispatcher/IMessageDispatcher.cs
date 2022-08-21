namespace FatedTimezoneBot.Logic.Dispatcher
{
    public interface IMessageDispatcher
    {
        void AddHandler(ICommandHandler c);
        void AddListener(IListener l);
    }
}