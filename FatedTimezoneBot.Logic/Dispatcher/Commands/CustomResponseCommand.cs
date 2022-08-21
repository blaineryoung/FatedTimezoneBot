using Discord;
using FatedTimezoneBot.Logic.Discord;
using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Serializers;
using FatedTimezoneBot.Logic.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Dispatcher.Commands
{
    public class CustomResponseCommand : ICommandHandler
    {
        private IChannelInformationFetcher channelInformationFetcher;
        private Random random = new Random();
        private ILogger<CustomResponseCommand> _logger;

        public CustomResponseCommand(IChannelInformationFetcher channelInformationFetcher, ILogger<CustomResponseCommand> logger)
        {
            this.channelInformationFetcher = channelInformationFetcher;
            this._logger = logger;
        }

        public async Task<bool> HandleCommand(IMessage message)
        {
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

            return await HandleCustomResponse(message, channelInformation);
        }

        private async Task<bool> HandleCustomResponse(IMessage message, ChannelInformation channelInformation)
        {
            IEnumerable<ChannelResponse> responses = channelInformation.CustomResponse.Where(x => message.Content.Contains(x.searchstring, StringComparison.OrdinalIgnoreCase));
            bool anyResponses = responses.Any();

            foreach (ChannelResponse response in responses)
            {
                if (response.frequency >= random.Next(100))
                {
                    EmbedBuilder eb = new EmbedBuilder();

                    eb.Description = response.response;
                    await message.Channel.SendMessageAsync("", false, eb.Build());
                }
            }
            
            return anyResponses;
        }
    }
}
