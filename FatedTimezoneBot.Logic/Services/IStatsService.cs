using Discord;
using FatedTimezoneBot.Logic.Information.Serializers;
using FatedTimezoneBot.Logic.Services.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Services
{
    public interface IStatsService
    {
        Task ProcessMessage(IMessage message);

        Task<ChannelStats> GetStats(ulong channelId);

        Task<PlayerStats> GetUserStats(ulong channelId, string user);

        Task<string> PrintChannelStats(ulong channelId);

        Task<string> PrintUserStats (ulong channelId, string user);

        Task UpdateCharacterInfo(ulong channelId, string userId, string userName, CharacterInfo ci);
    }
}
