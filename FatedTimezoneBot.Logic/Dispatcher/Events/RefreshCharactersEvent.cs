using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Exceptions;
using FatedTimezoneBot.Logic.Information.Serializers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Dispatcher.Events
{
    public class RefreshCharactersEvent : IEventHandler
    {
        public string Name => "RefreshCharacters";

        public ulong Interval => 1000 * 60 * 60; // one hour

        IChannelInformationFetcher channelInformationFetcher = null;
        ICharacterInformationFetcher characterInformationFetcher = null;
        IGearSetInformationFetcher gearSetInformationFetcher = null;
        ILogger _logger;
        
        public RefreshCharactersEvent(
            IChannelInformationFetcher channelInformationFetcher,
            ICharacterInformationFetcher characterInformationFetcher,
            IGearSetInformationFetcher gearSetInformationFetcher,
            ILogger logger)
        {
            this.channelInformationFetcher = channelInformationFetcher;
            this.characterInformationFetcher = characterInformationFetcher;
            this.gearSetInformationFetcher = gearSetInformationFetcher;
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
                            GearSetInfo gearSetInfo = await this.gearSetInformationFetcher.GetGearSetInformation(new Guid(c.bisid));
                            CharacterInfo characterInfo = await this.characterInformationFetcher.GetCharacterInformation(c.characterid, gearSetInfo.job);
                            _logger.Information("Refreshed {characterName}", characterInfo.Character.Name);
                        }
                        catch (CharacterNotFoundException e) 
                        {
                            _logger.Warning("Could not refresh {playerName} - {characterid} - {Message}", player.displayname, c.characterid, e.Message);
                        } // Not a big deal, just won't update.

                        await Task.Delay(1300);
                    }
                }
            }
        }
    }
}
