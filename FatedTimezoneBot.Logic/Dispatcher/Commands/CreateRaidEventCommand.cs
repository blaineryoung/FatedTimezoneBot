using Discord;
using FatedTimezoneBot.Logic.Information;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Dispatcher.Commands
{
    internal class CreateRaidEventCommand : ICommandHandler
    {
        private IChannelInformationFetcher channelInformationFetcher;
        private IDiscordClient client;

        ICollection<TimeZoneInfo> timeZones;

        private Regex CreateRaidEventCommandRegex = new Regex("(!createevent\\s*?(/G[a-z].*/i)");

        public CreateRaidEventCommand(IChannelInformationFetcher channelInformationFetcher, IDiscordClient client)
        {
            this.timeZones = TimeZoneInfo.GetSystemTimeZones();
            this.client = client;
            this.channelInformationFetcher = channelInformationFetcher;
        }

        public Task<bool> HandleCommand(IMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
