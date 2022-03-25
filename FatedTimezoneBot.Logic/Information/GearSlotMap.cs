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
        private IEnumerable<GearItem> gear;

        internal GearSlotMap(IEnumerable<GearItem> gear)
        {
            this.gear = gear;
        }

        public int Count => gear.Count();

        public IEnumerable<GearItem> Gear => gear;

        public static GearSlotMap GenerateDiff(GearSlotMap current, GearSlotMap expected)
        {
            IEnumerable<GearItem> diff = expected.Gear.Except(current.Gear);
            return new GearSlotMap(diff);
        }
    }
}
