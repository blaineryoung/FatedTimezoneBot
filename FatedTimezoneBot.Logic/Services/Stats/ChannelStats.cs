using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Services.Stats
{
    public class ChannelStats
    {
        public ulong ChannelId {get; set;}

        public DateTime StatStart {get; set;}

        public ConcurrentDictionary<string, PlayerStats> PlayerStatsCache { get; set;}

        public ChannelStats()
        {
            PlayerStatsCache = new ConcurrentDictionary<string, PlayerStats>();
        }

        public ChannelStats(ulong channelId)
        {
            this.ChannelId = channelId;
            this.StatStart = DateTime.UtcNow;
            PlayerStatsCache = new ConcurrentDictionary<string, PlayerStats>();
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static ChannelStats Deserialize(string input)
        {
            return JsonConvert.DeserializeObject<ChannelStats>(input);
        }

        public PlayerStats GetOrAddPlayer(string userId)
        {
            PlayerStats playerStats;

            if (false == this.PlayerStatsCache.TryGetValue(userId, out playerStats))
            {
                playerStats = new PlayerStats();
                playerStats.PlayerId = userId;
                playerStats.PlayerName = userId; // use this temporarily until we have a real object
                if (false == this.PlayerStatsCache.TryAdd(userId, playerStats))
                {
                    if (false == this.PlayerStatsCache.TryGetValue(userId, out playerStats))
                    {
                        throw new Exception($"Could not add player {userId}");
                    }
                }
            }

            return playerStats;
        }
    }

    public class StatLeaderLine
    {
        public string UserName {get; set;}
        public string StatName {get; set;}

        public int StatValue {get; set;}
    }
}
