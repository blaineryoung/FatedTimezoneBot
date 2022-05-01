using FatedTimezoneBot.Logic.Services.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Stores
{
    public interface IStatsStore
    {
        Task UpdateChannelStats(ChannelStats stats);

        Task<ChannelStats> GetStatsForChannel(ulong channelId);
    }
}
