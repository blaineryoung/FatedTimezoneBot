using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Discord
{
    public interface IDiscordClient
    {
        public delegate Task MessageReceivedHandler<IDiscordMessage>(IDiscordMessage message);

        Task Connect();

        public event MessageReceivedHandler<IDiscordMessage> MessageReceived;
    }
}
