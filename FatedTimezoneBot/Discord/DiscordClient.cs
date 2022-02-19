using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Discord
{
    public class DiscordClient : IDiscordClient
    {
        private DiscordSocketClient _client;
        private string _token;

        public event IDiscordClient.MessageReceivedHandler<IDiscordMessage> MessageReceived;

        public DiscordClient(string token)
        {
            _token = token;
        }

        public async Task Connect()
        {
            _client = new DiscordSocketClient();

            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();

            _client.MessageReceived += _client_MessageReceived;
            _client.Ready += () =>
            {
                Console.WriteLine("Bot is connected!");
                return Task.CompletedTask;
            };
        }

        private async Task _client_MessageReceived(SocketMessage arg)
        {
            IDiscordClient.MessageReceivedHandler<IDiscordMessage> handler = MessageReceived;

            DiscordSocketMessage m = new DiscordSocketMessage(arg);

            await handler?.Invoke(m);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
