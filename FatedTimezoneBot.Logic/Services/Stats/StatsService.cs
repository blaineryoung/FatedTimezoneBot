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

        public async Task FlushStatsForChannel(ulong channelId)
        {
            ChannelStats channelStats = await this._statsStore.GetStatsForChannel(channelId);
            await this._statsStore.UpdateChannelStats(channelStats);
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

            PlayerStats mostMessages = channelStats.PlayerStatsCache.Values.OrderByDescending(x => x.MessageCount).FirstOrDefault();
            int totalMessages = channelStats.PlayerStatsCache.Values.Select(x => x.MessageCount).Sum();
            if (null != mostMessages)
            {
                outputString.AppendLine($"Messages: **{totalMessages}**.  Most: {mostMessages.PlayerName}: {mostMessages.MessageCount}");
            }
            else
            {
                outputString.AppendLine($"Messages: **{totalMessages}**.");
            }

            PlayerStats mostWords = channelStats.PlayerStatsCache.Values.OrderByDescending(x => x.WordCount).FirstOrDefault();
            int totalWords = channelStats.PlayerStatsCache.Values.Select(x => x.WordCount).Sum();
            if (null != mostWords)
            {
                outputString.AppendLine($"Words: **{totalWords}**.  Most: {mostWords.PlayerName}: {mostWords.WordCount}");
            }
            else
            {
                outputString.AppendLine($"Words: **{totalWords}**.");
            }

            PlayerStats mostMounts = channelStats.PlayerStatsCache.Values.OrderByDescending(x => x.MountCount).FirstOrDefault();
            int totalMounts = channelStats.PlayerStatsCache.Values.Select(x => x.MountCount).Sum();
            if (null != mostMounts)
            {
                outputString.AppendLine($"Mounts: **{totalMounts}**.  Most: {mostMounts.PlayerName}: {mostMounts.MountCount}");
            }
            else
            {
                outputString.AppendLine($"Mounts: **{totalMounts}**.");
            }

            return outputString.ToString();
        }

        public async Task<string> PrintUserStats(ulong channelId, string user)
        {
            StringBuilder outputString = new StringBuilder();
            ChannelStats channelStats = await this._statsStore.GetStatsForChannel(channelId);
            PlayerStats playerStats = channelStats.GetOrAddPlayer(user);

            outputString.AppendLine($"Messages: **{playerStats.MessageCount}**.");
            outputString.AppendLine($"Words: **{playerStats.WordCount}**.");
            outputString.AppendLine($"Mounts: **{playerStats.MountCount}**.");

            return outputString.ToString();
        }

        public async Task ProcessMessage(IMessage message)
        {
            if (message.Author.IsBot)
            {
                return;
            }

            // Ignore commands
            if (message.Content.StartsWith('!'))
            {
                return;
            }

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

        public async Task ResetStatsForChannel(ulong channelId)
        {
            ChannelStats channelStats = new ChannelStats(channelId);
            await this._statsStore.UpdateChannelStats(channelStats);
        }

        public async Task UpdateCharacterInfo(ulong channelId, string userId, string userName, CharacterInfo ci)
        {
            ChannelStats channelStats = await this._statsStore.GetStatsForChannel(channelId);
            PlayerStats playerStats = channelStats.GetOrAddPlayer(userId);

            lock(playerStats)
            {
                playerStats.PlayerName = userName;
                if (ci.Mounts.Count() > playerStats.MountCount)
                {
                    playerStats.MountCount = ci.Mounts.Count();
                }
            }
        }
    }
}
