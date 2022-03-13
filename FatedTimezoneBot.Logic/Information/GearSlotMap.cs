using FatedTimezoneBot.Logic.Information.Serializers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information
{
    public class GearSlotMap
    {
        private IReadOnlyDictionary<string, GearItem> slotMappings;
        private IEnumerable<GearItem> rings;

        internal GearSlotMap(IReadOnlyDictionary<string,GearItem> slotMappings, IEnumerable<GearItem> rings)
        {
            this.slotMappings = slotMappings;
            this.rings = rings;
        }

        public GearItem this[string name]
        {
            get { return slotMappings[name]; }
        }

        public int Count => slotMappings.Count;

        public IEnumerable<GearItem> Rings => rings;

        public IEnumerable<string> Slots { get { return slotMappings.Keys; } }

        public static GearSlotMap GenerateDiff(GearSlotMap current, GearSlotMap expected)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            Dictionary<string, GearItem> diff = new Dictionary<string, GearItem>();

            foreach (string slot in expected.Slots)
            {
                if (current[slot] != expected[slot])
                {
                    diff.Add(slot, expected[slot]);
                }
            }

            List<GearItem> missingRings = new List<GearItem>();
            int ringDiffCount = 0;
            foreach (GearItem ring in expected.rings)
            {
                if (0 == current.rings.Where(x => x.id == ring.id).Count())
                {
                    diff.Add($"{textInfo.ToTitleCase(ring.slotName)}{++ringDiffCount}", ring);
                    missingRings.Add(ring);
                }
            }

            return new GearSlotMap(diff, missingRings);
        }
    }
}
