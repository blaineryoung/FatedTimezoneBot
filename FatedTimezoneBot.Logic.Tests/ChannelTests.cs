using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.FileFetchers;
using NUnit.Framework;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Tests
{
    public class ChannelTests
    {
        ILogger logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        [Test]
        public async Task CheckChannelIds()
        {
            IChannelInformationFetcher channelInformationFetcher = new ChannelFileInformationFetcher(logger);
            IEnumerable<ulong> channelIds = await channelInformationFetcher.GetAllChannelIds();

            Assert.AreEqual(2, channelIds.Count());
        }
    }
}
