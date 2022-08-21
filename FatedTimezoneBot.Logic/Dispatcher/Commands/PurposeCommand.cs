using Discord;
using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Serializers;
using FatedTimezoneBot.Logic.Utility;
using Microsoft.Extensions.Logging;

namespace FatedTimezoneBot.Logic.Dispatcher.Commands
{
    public class PurposeCommand : ICommandHandler
    {
        private IChannelInformationFetcher channelInformationFetcher;
        const string RaidTimesCommandString = "what is my purpose";
        private ILogger<PurposeCommand> _logger;

        ICollection<TimeZoneInfo> timeZones;

        public PurposeCommand(IChannelInformationFetcher channelInformationFetcher, ILogger<PurposeCommand> logger)
        {
            this.timeZones = TimeZoneInfo.GetSystemTimeZones();
            this.channelInformationFetcher = channelInformationFetcher;
            _logger = logger;
        }

        public async Task<bool> HandleCommand(IMessage message)
        {
            if (!message.Content.Contains(RaidTimesCommandString, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            ChannelInformation channelInformation = null;
            try
            {
                channelInformation = await channelInformationFetcher.GetChannelInformation(message.Channel.Id);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Attempting to load information for channel {channel}", message.Channel.Id);

                return false;
            }

            string userName = DiscordUtilities.GetDisambiguatedUser(message.Author);
            ChannelPlayer player;

            if (false == channelInformation.PlayerInformation.PlayerMap.TryGetValue(userName, out player))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(player.purpose))
            {
                EmbedBuilder eb = new EmbedBuilder();

                eb.Description = player.purpose;
                await message.Channel.SendMessageAsync("", false, eb.Build());
            }

            return true;
        }

    }
}
