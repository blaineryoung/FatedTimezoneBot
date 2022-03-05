using FatedTimezoneBot.Logic.Information.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information
{
    public class GearSetFileInformationFetcher : IGearSetInformationFetcher
    {

        // This is a cheesy in memory cache.  If this ever gets big, we'll need to do something better. 
        // It also doesn't handle file changes.  Simple thing to do would be add a file system watcher.  Later
        private Dictionary<Guid, GearSetInfo> gearSetCache = new Dictionary<Guid, GearSetInfo>();

        IGearInformationFetcher _gearFetcher;

        public GearSetFileInformationFetcher(IGearInformationFetcher gearFetcher)
        {
            this._gearFetcher = gearFetcher;
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

                gearSetCache.Add(gearSetId, gearSetInformation);
            }

            return gearSetInformation;
        }
    }
}
