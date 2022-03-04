using FatedTimezoneBot.Logic.Information.Serializers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace FatedTimezoneBot.Logic.Information
{
    public class GearSetInformation
    {
        public IReadOnlyDictionary<string, GearItem> SlotGear { get; private set; }

        public GearSetInformation(GearSetInfo setInfo, GearInformation gearInfo)
        {
            Dictionary<string, GearItem> slotGear = new Dictionary<string, GearItem>();
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            IEnumerable<PropertyInfo> props = setInfo.GetType().GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(SlotAttribute)));
            foreach (PropertyInfo prop in props)
            {
                string name = textInfo.ToTitleCase(prop.Name);
                object value = prop.GetValue(setInfo, null);

                if (null != value)
                {
                    int id = (int)value;
                    slotGear[name] = gearInfo.GearMap[id];
                }
            }

            this.SlotGear = new ReadOnlyDictionary<string, GearItem>(slotGear);
        }
    }
}
