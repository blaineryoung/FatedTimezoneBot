using FatedTimezoneBot.Logic.Information.Serializers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information
{
    public class GearSlotMapperFactory : IGearSlotMapperFactory
    {
        IGearInformationFetcher gearInformationFetcher;
        ILogger _logger;

        public GearSlotMapperFactory(IGearInformationFetcher gearInformationFetcher, ILogger logger)
        {
            this.gearInformationFetcher = gearInformationFetcher;
            this._logger = logger;
        }

        public async Task<GearSlotMap> CreateGearSlotMap(GearSetInfo setInfo)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            List<GearItem> gear = new List<GearItem>();

            IEnumerable<PropertyInfo> props = setInfo.GetType().GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(SlotAttribute)));
            foreach (PropertyInfo prop in props)
            {
                
                object value = prop.GetValue(setInfo, null);

                if (null != value)
                {
                    int id = (int)value;
                    GearItem item = await gearInformationFetcher.GetGearInformation(id);
                    gear.Add(item);
                }
            }

            return new GearSlotMap(gear);
        }

        public async  Task<GearSlotMap> CreateGearSlotMap(CharacterInfo characterInfo)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            IEnumerable<PropertyInfo> props = characterInfo.Character.GearSet.Gear.GetType().GetProperties();
            List<GearItem> gear = new List<GearItem>();

            foreach (PropertyInfo prop in props)
            {
                object value = prop.GetValue(characterInfo.Character.GearSet.Gear, null);

                if ((null != value) && (value is CharacterGearItem))
                {
                    CharacterGearItem cgi = (CharacterGearItem)value;
                    GearItem item = await gearInformationFetcher.GetGearInformation(cgi.ID);
                    gear.Add(item);
                }
            }

            return new GearSlotMap(gear);
        }
    }
}
