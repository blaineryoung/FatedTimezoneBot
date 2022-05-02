using Discord;
using FatedTimezoneBot.Logic.Discord;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Dispatcher
{
    public class MessageDispatcher
    {
        List<ICommandHandler> handlers = new List<ICommandHandler>();
        List<IListener> listeners = new List<IListener>();
        ILogger _logger;

        public MessageDispatcher(IDiscordClientWrapper client, ILogger logger)
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
