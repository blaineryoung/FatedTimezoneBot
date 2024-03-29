﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information.Serializers
{

    public class GearInfo
    {
        public GearItem[] GearItems { get; set; }

        public static GearInfo DeserializeGearData(string gearData)
        {
            GearItem[] gi =  JsonConvert.DeserializeObject<GearItem[]>(gearData);

            GearInfo g = new GearInfo();
            g.GearItems = gi;
            return g;
        }
    }

    public class GearItem : IEquatable<GearItem>
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int param0 { get; set; }
        public int? param1 { get; set; }
        public int? param2 { get; set; }
        public int? param3 { get; set; }
        public object param4 { get; set; }
        public object param5 { get; set; }
        public int param0Value { get; set; }
        public int param1Value { get; set; }
        public int param2Value { get; set; }
        public int param3Value { get; set; }
        public int param4Value { get; set; }
        public int param5Value { get; set; }
        public bool advancedMelding { get; set; }
        public int block { get; set; }
        public int blockRate { get; set; }
        public bool canBeHq { get; set; }
        public int damageMag { get; set; }
        public int damagePhys { get; set; }
        public int defenseMag { get; set; }
        public int defensePhys { get; set; }
        public int delay { get; set; }
        public int iconId { get; set; }
        public string iconPath { get; set; }
        public int itemLevel { get; set; }
        public int itemSpecialBonusParam { get; set; }
        public int level { get; set; }
        public int materiaSlotCount { get; set; }
        public int materializeType { get; set; }
        public bool PVP { get; set; }
        public int rarity { get; set; }
        public int slotCategory { get; set; }
        public bool unique { get; set; }
        public bool untradable { get; set; }
        public bool weapon { get; set; }
        public bool canCustomize { get; set; }
        public string slotName { get; set; }
        public string jobName { get; set; }
        public int itemUICategory { get; set; }
        public int jobCategory { get; set; }

        public bool Equals(GearItem? other)
        {
            if (other == null) return false;

            return other.id == id;
        }
    }
}
