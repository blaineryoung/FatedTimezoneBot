using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.FileFetchers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Tests
{
    public class ChannelTests
    {

        [Test]
        public async Task CheckChannelIds()
        {
            IChannelInformationFetcher channelInformationFetcher = new ChannelFileInformationFetcher(DiscordTestUtilities.GetLogger());
            IEnumerable<ulong> channelIds = await channelInformationFetcher.GetAllChannelIds();

            Assert.AreEqual(2, channelIds.Count());
        }
    }
}
