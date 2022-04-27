using FatedTimezoneBot.Logic.Information.Serializers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information.FileFetchers
{
    public class GearFileInformationFetcher : IGearInformationFetcher
    {
        public IReadOnlyDictionary<int, GearItem> GearMap { get; private set; }
        public ILogger _logger;

        public GearFileInformationFetcher(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<GearItem> GetGearInformation(int gearId)
        {
            if (GearMap == null)
            {
                string fileName = $"gamedata\\gear.json";
                string content;
                using (StreamReader sr = new StreamReader(fileName))
                {
                    content = await sr.ReadToEndAsync();
                }

                GearInfo info = GearInfo.DeserializeGearData(content);
                Dictionary<int, GearItem> map = new Dictionary<int, GearItem>();

                foreach (GearItem g in info.GearItems)
                {
                    map.Add(g.id, g);
                }

                GearMap = new ReadOnlyDictionary<int, GearItem>(map);
            }

            GearItem item = null;

            if (false == GearMap.TryGetValue(gearId, out item))
            {
                item = new GearItem() 
                { 
                    id = gearId,
                    name = "Unknown",
                };
            }

            return item;
        }
    }
}
