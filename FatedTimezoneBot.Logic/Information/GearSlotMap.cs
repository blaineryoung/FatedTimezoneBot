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

        public int Count => slotMappings.Count;

        public IEnumerable<string> Slots { get { return slotMappings.Keys; } }

        public static GearSlotMap GenerateDiff(GearSlotMap current, GearSlotMap expected)
        {
            Dictionary<string, GearItem> diff = new Dictionary<string, GearItem>();

            foreach (string slot in expected.Slots)
            {
                if (current[slot] != expected[slot])
                {
                    diff.Add(slot, expected[slot]);
                }
            }

            return new GearSlotMap(diff);
        }
    }
}
