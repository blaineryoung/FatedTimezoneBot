using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Discord
{
    internal class DiscordSocketMessage : IDiscordMessage
    {
        private SocketMessage _socketMessage;

        public DiscordSocketMessage(SocketMessage m)
        {
            this._socketMessage = m;
        }

        public bool IsBot => _socketMessage.Author.IsBot;

        public string UserName => $"{_socketMessage.Author.Username}#{_socketMessage.Author.Discriminator}";

        public string Content => _socketMessage.Content;

        public ulong ChannelId => _socketMessage.Channel.Id;

        public async Task SendEmbededMessageAsync(string message)
        {
            EmbedBuilder eb = new EmbedBuilder();

            eb.Description = message;
            await _socketMessage.Channel.SendMessageAsync("", false, eb.Build());
        }
    }
}
