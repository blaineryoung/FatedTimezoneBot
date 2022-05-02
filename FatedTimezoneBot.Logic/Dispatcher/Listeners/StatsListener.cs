using Discord;
using FatedTimezoneBot.Logic.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Dispatcher.Listeners
{
    public class StatsListener : IListener
    {
        private ILogger _logger;
        private IStatsService _statsService;

        public StatsListener(
            IStatsService statsService,
            ILogger logger)
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
