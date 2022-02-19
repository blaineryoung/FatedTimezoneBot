using FatedTimezoneBot.Logic.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Dispatcher
{
    internal interface ICommandHandler
    {
        /// <summary>
        /// Processes the message
        /// </summary>
        /// <param name="message"></param>
        /// <returns>True if the message was processed by this command, false otherwise</returns>
        Task<bool> HandleCommand(IDiscordMessage message);
    }
}
