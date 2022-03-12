using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Discord
{
    public interface IDiscordClientWrapper
    {
        public delegate Task MessageReceivedHandler<IDiscordMessage>(IMessage message);

        Task Connect();

        public event MessageReceivedHandler<IMessage> MessageReceived;

        IDiscordClient Client { get; }
    }
}
