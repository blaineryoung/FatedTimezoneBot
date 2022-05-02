using FatedTimezoneBot.Logic.Services.Stats;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Stores.FileStores
{
    public class FileStatsStore : IStatsStore
    {
        private ConcurrentDictionary<ulong, ChannelStats> channelCache = new ConcurrentDictionary<ulong, ChannelStats>();
        private ILogger _logger;

        public FileStatsStore(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<ChannelStats> GetStatsForChannel(ulong channelId)
        {
            ChannelStats channelStats = null;

            if (false == channelCache.TryGetValue(channelId, out channelStats))
            {
                _logger.Information("Channel stats for {channelId} not in cache", channelId);
                string fileName = $"channeldata\\{channelId}.stats.json";

                if (File.Exists(fileName))
                {
                    _logger.Information("Channel stats for {channelId} exists, loading from store", channelId);
                    string content;
                    using (StreamReader sr = new StreamReader(fileName))
                    {
                        content = await sr.ReadToEndAsync();
                    }

                    channelStats = ChannelStats.Deserialize(content);
                }
                else
                {
                    _logger.Information("Channel stats for {channelId} does not exist, creating new", channelId);
                    channelStats = new ChannelStats(channelId);
                }

                if (false == channelCache.TryAdd(channelId, channelStats))
                {
                    if (false == channelCache.TryGetValue(channelId, out channelStats))
                    {
                        Exception e = new Exception("Couldn't create new channel stats, but couldn't retrieve either");
                        _logger.Error(e, "Could not create stats for chanel {channelId}", channelId);
                    }
                }
            }

            return channelStats;
        }

        public async Task UpdateChannelStats(ChannelStats stats)
        {
            string fileName = $"channeldata\\{stats.ChannelId}.stats.json";
            string backupFileName = $"channeldata\\{stats.ChannelId}.stats.json.bak";

            // There should be one global channel stats, so no need to update the cache.
            // This of course will be horribly broken if we ever scale.
            
            // Make a backup of the old stats, if applicable.
            if (File.Exists(fileName))
            {
                if (File.Exists(backupFileName))
                {
                    File.Delete(backupFileName);
                }
                File.Copy(fileName, backupFileName, true);
            }

            string content = stats.Serialize();

            _logger.Information("Flushing stats for channel {channelId}", stats.ChannelId);
            using (StreamWriter sw = new StreamWriter(fileName, false))
            {
                try
                {
                    await sw.WriteAsync(content);
                    _logger.Information("Successfully flushed stats for channel {channelId}", stats.ChannelId);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Could not flush stats for channel {channelId}", stats.ChannelId);
                }
            }
        }
    }
}
