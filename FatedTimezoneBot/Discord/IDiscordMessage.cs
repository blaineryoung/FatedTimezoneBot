using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Discord
{
    public interface IDiscordMessage
    {
        bool IsBot { get; }

        string UserName { get; }

        string Content { get; }

        Task SendEmbededMessageAsync(string message);
    }
}
