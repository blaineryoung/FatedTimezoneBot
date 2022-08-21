using Discord;
using FatedTimezoneBot.Logic.Services;
using Microsoft.Extensions.Logging;

namespace FatedTimezoneBot.Logic.Dispatcher.Commands
{
    public class ShowChannelStatsCommand : ICommandHandler
    {
        private IStatsService statsService;
        const string ShowChannelStatsCommandString = "!channelstats";
        private ILogger<ShowChannelStatsCommand> _logger;

        public ShowChannelStatsCommand(IStatsService statsService, ILogger<ShowChannelStatsCommand> logger)
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
                _logger.LogWarning(e, "Attempting to load statistics for channel {channel}", message.Channel.Id);

                return false;
            }

            EmbedBuilder eb = new EmbedBuilder();

            eb.Description = statsString;
            await message.Channel.SendMessageAsync("", false, eb.Build());
            return true;
        }

    }
}
