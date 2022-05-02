using Discord;
using FatedTimezoneBot.Logic.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Dispatcher
{
    public interface IListener
    {
        /// <summary>
        /// Listeners passively listen to messages and do something with the contents.  They're run in
        /// parallel and should not send messages to the channel.
        /// </summary>
        /// <param name="message"></param>
        Task ProcessMessage(IMessage message);
    }
}
