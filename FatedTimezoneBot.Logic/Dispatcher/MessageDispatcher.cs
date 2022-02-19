using FatedTimezoneBot.Logic.Discord;
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

        public MessageDispatcher(IDiscordClient client)
        {
            client.MessageReceived += Client_MessageReceived;
        }

        private async Task Client_MessageReceived(IDiscordMessage message)
        {
            if (message.IsBot)
            {
                return;
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
    }
}
