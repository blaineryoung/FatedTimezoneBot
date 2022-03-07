using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Exceptions;
using FatedTimezoneBot.Logic.Information.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information.RestFetchers
{ 
    public class GearSetRestInformationFetcher : IGearSetInformationFetcher
    {

        // This is a cheesy in memory cache.  If this ever gets big, we'll need to do something better. 
        // It also doesn't handle file changes.  Simple thing to do would be add a file system watcher.  Later
        private Dictionary<Guid, GearSetInfo> gearSetCache = new Dictionary<Guid, GearSetInfo>();

        public GearSetRestInformationFetcher()
        {
        }

        public async Task<GearSetInfo> GetGearSetInformation(Guid gearSetId)
        {
            GearSetInfo gearSetInformation = null;
            if (false == gearSetCache.TryGetValue(gearSetId, out gearSetInformation))
            { 
                gearSetInformation = await GetGearInfoCall(gearSetId);
                gearSetCache.Add(gearSetId, gearSetInformation);
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
                    Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                    throw new GearNotFoundException($"GearSet id {gearSetId} not found - {response.StatusCode}, {response.ReasonPhrase}");
                }
            }
        }
    }
}
