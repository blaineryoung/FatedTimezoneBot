using FatedTimezoneBot.Logic.Information.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information
{
    public class GearSlotMap
    {
        private IReadOnlyDictionary<string, GearItem> slotMappings;

        internal GearSlotMap(IReadOnlyDictionary<string,GearItem> slotMappings)
        {
            this.slotMappings = slotMappings;
        }

        public GearItem this[string name]
        {
            get { return slotMappings[name]; }
        }

        public IEnumerable<string> Slots { get { return slotMappings.Keys; } }
    }
}
