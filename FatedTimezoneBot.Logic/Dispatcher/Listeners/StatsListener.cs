using Discord;
using FatedTimezoneBot.Logic.Services;
using Microsoft.Extensions.Logging;

namespace FatedTimezoneBot.Logic.Dispatcher.Listeners
{
    public class StatsListener : IListener
    {
        private ILogger<StatsListener> _logger;
        private IStatsService _statsService;

        public StatsListener(
            IStatsService statsService,
            ILogger<StatsListener> logger)
        {
            this._statsService = statsService;
            this._logger = logger;
        }

        public async Task ProcessMessage(IMessage message)
        {
            // Ignore commands
            if (message.Content.StartsWith('!'))
            {
                return;
            }

            await this._statsService.ProcessMessage(message);
        }
    }
}
