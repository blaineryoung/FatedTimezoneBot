using Discord;
using FatedTimezoneBot.Logic.Discord;
using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Serializers;
using FatedTimezoneBot.Logic.Utility;
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

        public CustomResponseCommand(IChannelInformationFetcher channelInformationFetcher)
        {
            this.channelInformationFetcher = channelInformationFetcher;
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
                Console.WriteLine($"Got exception {e.Message} when attempting to get channel information for {message.Channel.Id}");
                Console.WriteLine(e.StackTrace);

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
