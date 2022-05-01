using Discord;
using FatedTimezoneBot.Logic.Information.Serializers;
using FatedTimezoneBot.Logic.Stores;
using FatedTimezoneBot.Logic.Utility;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Services.Stats
{
    public class StatsService : IStatsService
    {
        private readonly ILogger _logger;
        private readonly IStatsStore _statsStore;

        public StatsService(IStatsStore store, ILogger logger)
        {
            this._statsStore = store;
            this._logger = logger;
        }

        public async Task<ChannelStats> GetStats(ulong channelId)
        {
            return await this._statsStore.GetStatsForChannel(channelId);
        }

        public async Task<PlayerStats> GetUserStats(ulong channelId, string user)
        {
            ChannelStats channelStats =  await this._statsStore.GetStatsForChannel(channelId);
            return channelStats.GetOrAddPlayer(user);
        }

        public async Task<string> PrintChannelStats(ulong channelId)
        {
            StringBuilder outputString = new StringBuilder();
            ChannelStats channelStats = await this._statsStore.GetStatsForChannel(channelId);

            outputString.AppendLine($"Stats as of {channelStats.StatStart.ToString("F")} (UTC)");

            PlayerStats mostMessages = channelStats.PlayerStats.Values.OrderByDescending(x => x.MessageCount).First();
            int totalMessages = channelStats.PlayerStats.Values.Select(x => x.MessageCount).Sum();
            outputString.AppendLine($"Messages: **{totalMessages}**.  Most: {mostMessages.PlayerName}: {mostMessages.MessageCount}");

            PlayerStats mostWords = channelStats.PlayerStats.Values.OrderByDescending(x => x.WordCount).First();
            int totalWords = channelStats.PlayerStats.Values.Select(x => x.WordCount).Sum();
            outputString.AppendLine($"Words: **{totalWords}**.  Most: {mostWords.PlayerName}: {mostWords.WordCount}");

            PlayerStats mostMounts = channelStats.PlayerStats.Values.OrderByDescending(x => x.MountCount).First();
            int totalMounts = channelStats.PlayerStats.Values.Select(x => x.MountCount).Sum();
            outputString.AppendLine($"Mounts: **{totalMounts}**.  Most: {mostMounts.PlayerName}: {mostMounts.MountCount}");

            return outputString.ToString();
        }

        public async Task<string> PrintUserStats(ulong channelId, string user)
        {
            StringBuilder outputString = new StringBuilder();
            ChannelStats channelStats = await this._statsStore.GetStatsForChannel(channelId);
            PlayerStats playerStats = channelStats.GetOrAddPlayer(user);

            outputString.AppendLine($"Stats for {playerStats.PlayerName} as of {channelStats.StatStart.ToString("F")} (UTC)");
            outputString.AppendLine($"Messages: **{playerStats.MessageCount}**.");
            outputString.AppendLine($"Words: **{playerStats.WordCount}**.");
            outputString.AppendLine($"Mounts: **{playerStats.MountCount}**.");

            return outputString.ToString();
        }

        public async Task ProcessMessage(IMessage message)
        {
            string userId = DiscordUtilities.GetDisambiguatedUser(message.Author);
            ulong channelId = message.Channel.Id;

            int wordCount = message.Content.Split(' ', '\n').Count();

            ChannelStats channelStats = await this._statsStore.GetStatsForChannel(channelId);
            PlayerStats playerStats = channelStats.GetOrAddPlayer(userId);

            lock (playerStats)
            {
                playerStats.MessageCount += 1;
                playerStats.WordCount += wordCount;
            }
        }

        public async Task UpdateCharacterInfo(ulong channelId, string userId, string userName, CharacterInfo ci)
        {
            ChannelStats channelStats = await this._statsStore.GetStatsForChannel(channelId);
            PlayerStats playerStats = channelStats.GetOrAddPlayer(userId);

            lock(playerStats)
            {
                playerStats.PlayerName = userName;
                playerStats.MountCount = ci.Mounts.Count();
            }
        }
    }
}
