using FatedTimezoneBot.Logic.Information.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace FatedTimezoneBot.Logic.Information
{
    public class GearInformation
    {
        public IReadOnlyDictionary<int, GearItem> GearMap { get; private set; }

        internal GearInformation(GearInfo gear)
        {
            Dictionary<int, GearItem> map = new Dictionary<int, GearItem>();

            foreach(GearItem g in gear.GearItems)
            {
                map.Add(g.id, g);
            }

            GearMap = new ReadOnlyDictionary<int, GearItem>(map);
        }
    }

}
