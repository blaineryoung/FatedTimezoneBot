﻿using FatedTimezoneBot.Logic.Information.Serializers;
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

        public GearSlotMapperFactory(IGearInformationFetcher gearInformationFetcher)
        {
            this.gearInformationFetcher = gearInformationFetcher;
        }

        public async Task<GearSlotMap> CreateGearSlotMap(GearSetInfo setInfo)
        {
            Dictionary<string, GearItem> slotGear = new Dictionary<string, GearItem>();
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            IEnumerable<PropertyInfo> props = setInfo.GetType().GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(SlotAttribute)));
            foreach (PropertyInfo prop in props)
            {
                
                object value = prop.GetValue(setInfo, null);

                if (null != value)
                {
                    int id = (int)value;
                    GearItem item = await gearInformationFetcher.GetGearInformation(id);
                    string name = textInfo.ToTitleCase(item.slotName);
                    slotGear[name] = item;
                }
            }

            return new GearSlotMap(new ReadOnlyDictionary<string, GearItem>(slotGear));
        }

        public async  Task<GearSlotMap> CreateGearSlotMap(CharacterInfo characterInfo)
        {
            Dictionary<string, GearItem> slotGear = new Dictionary<string, GearItem>();
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            IEnumerable<PropertyInfo> props = characterInfo.Character.GearSet.GetType().GetProperties();

            foreach (PropertyInfo prop in props)
            {
                object value = prop.GetValue(characterInfo.Character.GearSet, null);

                if (null != value)
                {
                    int id = (int)value;
                    GearItem item = await gearInformationFetcher.GetGearInformation(id);
                    string name = textInfo.ToTitleCase(item.slotName);
                    slotGear[name] = item;
                }
            }

            return new GearSlotMap(new ReadOnlyDictionary<string, GearItem>(slotGear));
        }
    }
}