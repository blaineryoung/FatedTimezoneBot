using FatedTimezoneBot.Logic.Information.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information
{
    public class GearFileInformationFetcher : IGearInformationFetcher
    {
        private GearInformation gi = null;

        public async Task<GearInformation> GetGearInformation()
        {
            if (gi == null)
            {
                string fileName = $"gamedata\\gear.json";
                string content;
                using (StreamReader sr = new StreamReader(fileName))
                {
                    content = await sr.ReadToEndAsync();
                }

                GearInfo info = GearInfo.DeserializeGearData(content);
                gi = new GearInformation(info);
            }

            return gi;
        }
    }
}
