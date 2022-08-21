using Discord;
using FatedTimezoneBot.Logic.Discord;
using Microsoft.Extensions.Logging;

namespace FatedTimezoneBot.Logic.Dispatcher
{
    public class MessageDispatcher : IMessageDispatcher
    {
        List<ICommandHandler> handlers = new List<ICommandHandler>();
        List<IListener> listeners = new List<IListener>();
        ILogger<MessageDispatcher> _logger;

        public MessageDispatcher(IDiscordClientWrapper client, ILogger<MessageDispatcher> logger)
        {
            client.MessageReceived += Client_MessageReceived;
            this._logger = logger;
        }

        private async Task Client_MessageReceived(IMessage message)
        {
            if (message.Author.IsBot)
            {
                return;
            }

            foreach (IListener listener in listeners)
            {
                // Just fire it off.
                listener.ProcessMessage(message).ConfigureAwait(false);
            }

            foreach (ICommandHandler h in handlers)
            {
                try
                {
                    bool messageProcessed = await h.HandleCommand(message);
                    if (messageProcessed)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public void AddHandler(ICommandHandler c)
        {
            handlers.Add(c);
        }

        public void AddListener(IListener l)
        {
            listeners.Add(l);
        }
    }
}
