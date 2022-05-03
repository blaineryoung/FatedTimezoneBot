using Discord;
using FatedTimezoneBot.Logic.Discord;
using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Exceptions;
using FatedTimezoneBot.Logic.Information.Serializers;
using FatedTimezoneBot.Logic.Services;
using FatedTimezoneBot.Logic.Utility;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Dispatcher.Commands
{
    public class ResetStatsCommand : ICommandHandler
    {
        private IStatsService statsService;
        const string ShowChannelStatsCommandString = "!rebuildstats";

        IChannelInformationFetcher channelInformationFetcher = null;
        ICharacterInformationFetcher characterInformationFetcher = null;

        private ILogger _logger;

        public ResetStatsCommand(
            IChannelInformationFetcher channelInformationFetcher,
            ICharacterInformationFetcher characterInformationFetcher, 
            IStatsService statsService, 
            ILogger logger)
        {
            this.statsService = statsService;
            _logger = logger;
            this.channelInformationFetcher = channelInformationFetcher;
            this.characterInformationFetcher = characterInformationFetcher;
        }

        public async Task<bool> HandleCommand(IMessage message)
        {
            if (!message.Content.Contains(ShowChannelStatsCommandString, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (0 != string.Compare(DiscordUtilities.GetDisambiguatedUser(message.Author), "DaktCole#3699", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Warning("{baduser} tried to reset stats.  That's bad and not Dakt", DiscordUtilities.GetDisambiguatedUser(message.Author));
                return false;
            }

            string statsString;
            try
            {
                await statsService.ResetStatsForChannel(message.Channel.Id);
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Attempting to reset statistics for channel {channel}", message.Channel.Id);

                return false;
            }

            await message.Channel.SendMessageAsync("Rebuilding stats", false);

            _logger.Information("Refreshing characters");
            ChannelInformation channelInformation = await this.channelInformationFetcher.GetChannelInformation(message.Channel.Id);
            foreach (ChannelPlayer player in channelInformation.PlayerInformation.PlayerMap.Values)
            {
                // Simply fetch each character.  This should update the cache.
                foreach (ChannelCharacter c in player.characters)
                {
                    try
                    {
                        CharacterInfo characterInfo = await this.characterInformationFetcher.GetCharacterInformation(c.characterid);

                        await this.statsService.UpdateCharacterInfo(message.Channel.Id, player.username, player.displayname, characterInfo);

                        _logger.Information("Refreshed {characterName}", characterInfo.Character.Name);
                    }
                    catch (CharacterNotFoundException e)
                    {
                        _logger.Warning("Could not refresh {playerName} - {characterid} - {Message}", player.displayname, c.characterid, e.Message);
                    } // Not a big deal, just won't update.
                }
            }

            bool moreMessages = true;
            ulong currentPointer = message.Id;
            int currentCount = 0;
            while (moreMessages)
            {
                moreMessages = false;
                var messages = await message.Channel.GetMessagesAsync(currentPointer, Direction.Before).FlattenAsync<IMessage>();
                foreach (var m in messages.OrderByDescending(x => x.Id))
                {
                    await this.statsService.ProcessMessage(m);
                    moreMessages = true;
                    currentPointer = m.Id;
                    currentCount++;
                }
                _logger.Information("Processed {count} messages for channel {channelId}", currentCount, message.Channel.Id);
            }
            _logger.Information("Refresh complete");
            await message.Channel.SendMessageAsync("Rebuild complete", false);

            _logger.Information("Flushing stats for channel {channelId}", message.Channel.Id);
            try
            {
                await this.statsService.FlushStatsForChannel(message.Channel.Id);
                _logger.Information("Channel stats for channel {channelId} written to storage", message.Channel.Id);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Could not flush channel stats for channel {channelId} to storage", message.Channel.Id);
            }

            return true;
        }

    }
}
