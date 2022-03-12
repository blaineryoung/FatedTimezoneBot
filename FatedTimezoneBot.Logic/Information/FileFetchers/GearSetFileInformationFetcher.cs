using FatedTimezoneBot.Logic.Information.Serializers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information.FileFetchers
{
    public class GearSetFileInformationFetcher : IGearSetInformationFetcher
    {

        // This is a cheesy in memory cache.  If this ever gets big, we'll need to do something better. 
        // It also doesn't handle file changes.  Simple thing to do would be add a file system watcher.  Later
        private ConcurrentDictionary<Guid, GearSetInfo> gearSetCache = new ConcurrentDictionary<Guid, GearSetInfo>();

        public GearSetFileInformationFetcher()
        {
        }

        public async Task<GearSetInfo> GetGearSetInformation(Guid gearSetId)
        {
            GearSetInfo gearSetInformation = null;
            if (false == gearSetCache.TryGetValue(gearSetId, out gearSetInformation))
            {
                string fileName = $"gamedata\\gearsets\\{gearSetId}.json";
                string content;
                using (StreamReader sr = new StreamReader(fileName))
                {
                    content = await sr.ReadToEndAsync();
                }

                gearSetInformation = GearSetInfo.Deserialize(content);

                gearSetCache.TryAdd(gearSetId, gearSetInformation);
            }

            return gearSetInformation;
        }
    }
}
