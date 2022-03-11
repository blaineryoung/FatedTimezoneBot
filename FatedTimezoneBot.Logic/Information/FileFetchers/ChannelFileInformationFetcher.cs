using FatedTimezoneBot.Logic.Information.Serializers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information.FileFetchers
{
    public class ChannelFileInformationFetcher : IChannelInformationFetcher
    {
        // This is a cheesy in memory cache.  If this ever gets big, we'll need to do something better. 
        // It also doesn't handle file changes.  Simple thing to do would be add a file system watcher.  Later
        private ConcurrentDictionary<ulong, ChannelInformation> channelCache = new ConcurrentDictionary<ulong, ChannelInformation>();

        public async Task<IEnumerable<ulong>> GetAllChannelIds()
        {
            List<ulong> channelIds = new List<ulong>();

            foreach (string file in Directory.EnumerateFiles("channeldata\\"))
            {
                string[] tokens = file.Split('.');
                ulong channelId;
                if (false != ulong.TryParse(tokens[0].Split('\\')[1], out channelId))
                {
                    channelIds.Add(channelId);
                }
            }

            return channelIds;
        }

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

                channelCache.TryAdd(channelId, channelInformation);
            }

            return channelInformation;
        }
    }
}
