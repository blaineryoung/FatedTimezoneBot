using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information.Serializers
{
    public class RaidInfo
    {
        public Raid[] raids { get; set; }

        public static async Task<IEnumerable<Raid>> LoadRaidFile(string raidFile)
        {
            string content;
            using (StreamReader sr = new StreamReader(raidFile))
            {
                content = await sr.ReadToEndAsync();
            }

            RaidInfo list = JsonConvert.DeserializeObject<RaidInfo>(content);
            return list.raids.AsEnumerable();
        }
    }

    public class Raid
    {
        public string day { get; set; }
        public string timezoneid { get; set; }
        public string time { get; set; }
    }

}
