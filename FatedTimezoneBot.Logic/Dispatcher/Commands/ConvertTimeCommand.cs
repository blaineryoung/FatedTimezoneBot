using Discord;
using FatedTimezoneBot.Logic.Discord;
using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Dispatcher.Commands
{
    public class ConvertTimeCommand : ICommandHandler
    {
        private IChannelInformationFetcher channelInformationFetcher;

        public ConvertTimeCommand(IChannelInformationFetcher channelInformationFetcher)
        {
            this.channelInformationFetcher = channelInformationFetcher;
        }

        public async Task<bool> HandleCommand(IMessage message)
        {
            if (!TimeUtilities.ContainsTime(message.Content))
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
                Console.WriteLine($"Got exception {e.Message} when attempting to get channel information for {message.Channel.Id}");
                Console.WriteLine(e.StackTrace);

                return false;
            }

            return await OutputTimes(message, channelInformation);
        }

        private async Task<bool> OutputTimes(IMessage message, ChannelInformation channelInformation)
        {
            string userName = DiscordUtilities.GetDisambiguatedUser(message.Author);

            TimeZoneInfo tz;
            if (false == channelInformation.PlayerInformation.TimeZoneMappings.TryGetValue(userName, out tz))
            {
                Console.WriteLine($"User {userName} not found");
                return false;
            }

            DateTime userTime = TimeUtilities.GetTimeFromText(message.Content, tz);

            StringBuilder sb = TimeUtilities.PrintUserTimes(userTime, tz, channelInformation.PlayerInformation.DisplayMappings);

            EmbedBuilder eb = new EmbedBuilder();

            eb.Description = sb.ToString();
            await message.Channel.SendMessageAsync("", false, eb.Build());

            return true;
        }
    }
}
