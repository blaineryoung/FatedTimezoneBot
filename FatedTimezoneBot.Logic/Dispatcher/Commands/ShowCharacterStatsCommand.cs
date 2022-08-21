using Discord;
using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Serializers;
using FatedTimezoneBot.Logic.Services;
using FatedTimezoneBot.Logic.Utility;
using Microsoft.Extensions.Logging;

namespace FatedTimezoneBot.Logic.Dispatcher.Commands
{
    public class ShowCharacterStatsCommand : ICommandHandler
    {
        private IStatsService statsService;
        private IChannelInformationFetcher _channelInformationFetcher;
        const string ShowChannelStatsCommandString = "!characterstats";
        private ILogger<ShowCharacterStatsCommand> _logger;

        public ShowCharacterStatsCommand(
            IChannelInformationFetcher channelInformationFetcher, 
            IStatsService statsService,
            ILogger<ShowCharacterStatsCommand> logger)
        {
            this.statsService = statsService;
            this._channelInformationFetcher = channelInformationFetcher;
            _logger = logger;
        }

        public async Task<bool> HandleCommand(IMessage message)
        {
            if (!message.Content.Contains(ShowChannelStatsCommandString, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            ChannelInformation channelInformation = null;
            try
            {
                channelInformation = await _channelInformationFetcher.GetChannelInformation(message.Channel.Id);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Attempting to load information for channel {channel}", message.Channel.Id);

                return false;
            }

            string[] commandArgs = message.Content.Split(' ');

            string user;
            ChannelPlayer player;

            if (commandArgs.Length == 1)
            {
                user = DiscordUtilities.GetDisambiguatedUser(message.Author);
                if (false == channelInformation.PlayerInformation.PlayerMap.TryGetValue(user, out player))
                {
                    _logger.LogWarning("Could not find information for user {user}, skipping stats", user);
                }
            }
            else
            {
                if (false == channelInformation.PlayerInformation.PlayerDisplayNameMap.TryGetValue(commandArgs[1], out player))
                {
                    _logger.LogWarning("Could not find information for user {user}, skipping stats", commandArgs[1]);
                }
                user = channelInformation.PlayerInformation.PlayerDisplayNameMap[commandArgs[1]].username;
            }

            string statsString;
            try
            {
                statsString = await statsService.PrintUserStats(message.Channel.Id, user);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Attempting to load statistics for player {player}", user);

                return false;
            }

            EmbedBuilder eb = new EmbedBuilder();

            eb.Description = statsString;
            await message.Channel.SendMessageAsync("", false, eb.Build());
            return true;
        }

    }
}
