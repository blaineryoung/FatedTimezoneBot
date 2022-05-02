using Discord;
using FatedTimezoneBot.Logic.Discord;
using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Serializers;
using FatedTimezoneBot.Logic.Services;
using FatedTimezoneBot.Logic.Utility;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Dispatcher.Commands
{
    public class ShowChannelStatsCommand : ICommandHandler
    {
        private IStatsService statsService;
        const string ShowChannelStatsCommandString = "!channelstats";
        private ILogger _logger;

        public ShowChannelStatsCommand(IStatsService statsService, ILogger logger)
        {
            this.statsService = statsService;
            _logger = logger;
        }

        public async Task<bool> HandleCommand(IMessage message)
        {
            if (!message.Content.Contains(ShowChannelStatsCommandString, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string statsString;
            try
            {
                statsString = await statsService.PrintChannelStats(message.Channel.Id);
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Attempting to load statistics for channel {channel}", message.Channel.Id);

                return false;
            }

            EmbedBuilder eb = new EmbedBuilder();

            eb.Description = statsString;
            await message.Channel.SendMessageAsync("", false, eb.Build());
            return true;
        }

    }
}
