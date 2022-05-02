using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Exceptions;
using FatedTimezoneBot.Logic.Information.Serializers;
using FatedTimezoneBot.Logic.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Dispatcher.Events
{
    public class UpdateStatisticsEvent : IEventHandler
    {
        public string Name => "UpdateStatistics";

        public ulong Interval => 1000 * 60 * 60; // one hour

        public bool RunAtStart => true;

        IChannelInformationFetcher channelInformationFetcher = null;
        ICharacterInformationFetcher characterInformationFetcher = null;
        IGearSetInformationFetcher gearSetInformationFetcher = null;
        IStatsService statsService = null;
        ILogger _logger;
        
        public UpdateStatisticsEvent(
            IChannelInformationFetcher channelInformationFetcher,
            ICharacterInformationFetcher characterInformationFetcher,
            IStatsService statsService,
            ILogger logger)
        {
            this.channelInformationFetcher = channelInformationFetcher;
            this.characterInformationFetcher = characterInformationFetcher;
            this.gearSetInformationFetcher = gearSetInformationFetcher;
            this.statsService = statsService;
            this._logger = logger;
        }

        public async Task HandleEvent()
        {
            IEnumerable<ulong> channelIds = await this.channelInformationFetcher.GetAllChannelIds();

            foreach (ulong channelId in channelIds)
            {
                ChannelInformation channelInformation = await this.channelInformationFetcher.GetChannelInformation(channelId);
                foreach (ChannelPlayer player in channelInformation.PlayerInformation.PlayerMap.Values)
                {
                    // Simply fetch each character.  This should update the cache.
                    foreach (ChannelCharacter c in player.characters)
                    {
                        try
                        {
                            CharacterInfo characterInfo = await this.characterInformationFetcher.GetCharacterInformation(c.characterid);

                            await this.statsService.UpdateCharacterInfo(channelId, player.username, player.displayname, characterInfo);

                            _logger.Information("Refreshed {characterName}", characterInfo.Character.Name);
                        }
                        catch (CharacterNotFoundException e) 
                        {
                            _logger.Warning("Could not refresh {playerName} - {characterid} - {Message}", player.displayname, c.characterid, e.Message);
                        } // Not a big deal, just won't update.

                        await Task.Delay(1300);
                    }
                }

                _logger.Information("Flushing stats for channel {channelId}", channelId);
                try
                {
                    await this.statsService.FlushStatsForChannel(channelId);
                    _logger.Information("Channel stats for channel {channelId} written to storage", channelId);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Could not flush channel stats for channel {channelId} to storage", channelId);
                }
            }
        }
    }
}
