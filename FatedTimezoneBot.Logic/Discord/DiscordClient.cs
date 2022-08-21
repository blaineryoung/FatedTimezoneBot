using Discord;
using Discord.WebSocket;

namespace FatedTimezoneBot.Logic.Discord
{
    public class DiscordClient : IDiscordClientWrapper
    {
        private DiscordSocketClient _client;
        private string _token;

        public event IDiscordClientWrapper.MessageReceivedHandler<IMessage> MessageReceived;

        public IDiscordClient Client => _client;

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

        private async Task _client_MessageReceived(IMessage arg)
        {
            IDiscordClientWrapper.MessageReceivedHandler<IMessage> handler = MessageReceived;
            await handler?.Invoke(arg);
        }

        private Task Log(LogMessage msg)
        {
            return Task.CompletedTask;
        }
    }
}
