using FatedTimezoneBot.Logic.Information.Exceptions;
using FatedTimezoneBot.Logic.Information.Serializers;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.Http.Headers;

namespace FatedTimezoneBot.Logic.Information.RestFetchers
{
    public class GearSetRestInformationFetcher : IGearSetInformationFetcher
    {

        // This is a cheesy in memory cache.  If this ever gets big, we'll need to do something better. 
        // It also doesn't handle file changes.  Simple thing to do would be add a file system watcher.  Later
        private ConcurrentDictionary<Guid, GearSetInfo> gearSetCache = new ConcurrentDictionary<Guid, GearSetInfo>();
        private ILogger<GearSetRestInformationFetcher> _logger;

        public GearSetRestInformationFetcher(ILogger<GearSetRestInformationFetcher> logger)
        {
            _logger = logger;
        }

        public async Task<GearSetInfo> GetGearSetInformation(Guid gearSetId)
        {
            GearSetInfo gearSetInformation = null;
            if (false == gearSetCache.TryGetValue(gearSetId, out gearSetInformation))
            { 
                gearSetInformation = await GetGearInfoCall(gearSetId);
                gearSetCache.TryAdd(gearSetId, gearSetInformation);
            }

            return gearSetInformation;
        }

        private async Task<GearSetInfo> GetGearInfoCall(Guid gearSetId)
        {
            string characterUri = $"https://etro.gg/api/gearsets/{gearSetId}";

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
                    return GearSetInfo.Deserialize(result);
                }
                else
                {
                    _logger.LogWarning("GearSet id {gearSetId} not found - {response.StatusCode}, {response.ReasonPhrase}", gearSetId, (int)response.StatusCode, response.ReasonPhrase);
                    throw new GearNotFoundException($"GearSet id {gearSetId} not found - {response.StatusCode}, {response.ReasonPhrase}");
                }
            }
        }
    }
}
