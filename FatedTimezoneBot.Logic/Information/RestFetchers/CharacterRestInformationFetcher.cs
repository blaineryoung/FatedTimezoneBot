using FatedTimezoneBot.Logic.Information.Exceptions;
using FatedTimezoneBot.Logic.Information.Serializers;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information.RestFetchers
{
    public class CharacterRestInformationFetcher : ICharacterInformationFetcher
    {
        // This is a cheesy in memory cache.  If this ever gets big, we'll need to do something better. 
        private ConcurrentDictionary<int, CharacterInfoCache> characterCache = new ConcurrentDictionary<int, CharacterInfoCache>();
        const int characterAgeMaxMinutes = 30;
        private ILogger _logger;

        public CharacterRestInformationFetcher(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<CharacterInfo> GetCharacterInformation(int characterId, int jobId = 0)
        {

            CharacterInfoCache characterInfo = null;

            // Not present
            if (false == characterCache.TryGetValue(characterId, out characterInfo))
            {
                CharacterInfo info = await this.GetCharacterInfoCall(characterId);
                if((0 != jobId) && (info.Character.ActiveClassJob.JobID != jobId))
                {
                    throw new CharacterNotFoundException($"Character {characterId} found, but active class not correct. Want {jobId}, got {info.Character.ActiveClassJob.ClassID}");
                }

                // don't cache the character if we didn't ask for job
                if (jobId != 0)
                {
                    characterInfo = new CharacterInfoCache(info);
                    characterCache.TryAdd(characterId, characterInfo);
                }

                return info;
            }

            // Present but old
            TimeSpan characterAge = DateTime.UtcNow.Subtract(characterInfo.LastUpdatedTime);
            if (characterAge.TotalMinutes > characterAgeMaxMinutes)
            {
                CharacterInfo info = await this.GetCharacterInfoCall(characterId);
                if ((0 != jobId) && (info.Character.ActiveClassJob.JobID != jobId))
                {
                    // don't update if class is wrong.
                    return characterInfo.CharacterInfo;
                }

                // only update cache if we asked for the job.
                if (jobId != 0)
                {
                    CharacterInfoCache characterInfoNew = new CharacterInfoCache(info);
                    characterCache.TryUpdate(characterId, characterInfoNew, characterInfo);
                }

                return info;
            }    

            return characterInfo.CharacterInfo;
        }
        
        private async Task<CharacterInfo> GetCharacterInfoCall(int characterId)
        {
            string characterUri = $"https://xivapi.com/character/{characterId}?data=MIMO";

            using (HttpClient client = new HttpClient())
            {
                Uri address = new Uri(characterUri);

                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                // List data response.
                HttpResponseMessage response = await client.GetAsync(address);  
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body.
                    string result = await response.Content.ReadAsStringAsync();
                    return CharacterInfo.Deserialize(result);
                }
                else
                {
                    _logger.Warning("Could not retrieve character {id} - {status} ({code})", characterId, (int)response.StatusCode, response.ReasonPhrase);
                    throw new CharacterNotFoundException($"Character id {characterId} not found - {response.StatusCode}, {response.ReasonPhrase}");
                }
            }
        }
    }

    internal class CharacterInfoCache
    {
        internal CharacterInfoCache(CharacterInfo info)
        {
            this.LastUpdatedTime = DateTime.UtcNow;
            this.CharacterInfo = info;
        }

        internal DateTime LastUpdatedTime { get; private set; }

        internal CharacterInfo CharacterInfo { get; private set; }
    }
}
