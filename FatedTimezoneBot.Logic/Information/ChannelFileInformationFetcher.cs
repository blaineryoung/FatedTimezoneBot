using FatedTimezoneBot.Logic.Information.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information
{
    public class ChannelFileInformationFetcher : IChannelInformationFetcher
    {
        // This is a cheesy in memory cache.  If this ever gets big, we'll need to do something better. 
        // It also doesn't handle file changes.  Simple thing to do would be add a file system watcher.  Later
        private Dictionary<ulong, ChannelInformation> channelCache = new Dictionary<ulong, ChannelInformation>();

        public async Task<ChannelInformation> GetChannelInformation(ulong channelId)
        {
            ChannelInformation channelInformation = null;
            if (false == channelCache.TryGetValue(channelId, out channelInformation))
            {
                string fileName = $"channeldata\\{channelId}.json";
                string content;
                using (StreamReader sr = new StreamReader(fileName))
                {
                    content = await sr.ReadToEndAsync();
                }

                ChannelInfo info = ChannelInfo.DeserializeChannelInfo(content);
                channelInformation = new ChannelInformation(info);

                channelCache.Add(channelId, channelInformation);
            }

            return channelInformation;
        }
    }
}
