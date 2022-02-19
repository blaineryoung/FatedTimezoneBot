using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Discord
{
    public interface IDiscordMessage
    {
        ulong ChannelId { get; }

        bool IsBot { get; }

        string UserName { get; }

        string Content { get; }

        Task SendEmbededMessageAsync(string message);
    }
}
