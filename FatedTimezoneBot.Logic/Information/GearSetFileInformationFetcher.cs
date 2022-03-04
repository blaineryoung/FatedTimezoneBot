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
        private Dictionary<Guid, GearSetInformation> gearSetCache = new Dictionary<Guid, GearSetInformation>();

        IGearInformationFetcher _gearFetcher;

        public GearSetFileInformationFetcher(IGearInformationFetcher gearFetcher)
        {
            this._gearFetcher = gearFetcher;
        }

        public async Task<GearSetInformation> GetGearSetInformation(Guid gearSetId)
        {
            GearSetInformation gearSetInformation = null;
            if (false == gearSetCache.TryGetValue(gearSetId, out gearSetInformation))
            {
                string fileName = $"gamedata\\gearsets\\{gearSetId}.json";
                string content;
                using (StreamReader sr = new StreamReader(fileName))
                {
                    content = await sr.ReadToEndAsync();
                }

                GearSetInfo gsi = GearSetInfo.Deserialize(content);
                GearInformation gearInfo = await _gearFetcher.GetGearInformation();

                gearSetInformation = new GearSetInformation(gsi, gearInfo);

                gearSetCache.Add(gearSetId, gearSetInformation);
            }

            return gearSetInformation;
        }
    }
}
