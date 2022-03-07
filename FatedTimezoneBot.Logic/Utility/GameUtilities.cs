using FatedTimezoneBot.Logic.Information.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Utility
{
    public static class GameUtilities
    {
        public const string Crafted = "Crafted";
        public const string Tomes = "Tomes";
        public const string P1S = "P1S";
        public const string P2S = "P2S";
        public const string P3S = "P3S";
        public const string P4S = "P4S";
        public const string Unknown = "Unknown";


        public static string GetGearSource(GearItem item)
        {
            // It's a bit hacky to hard code this, but sufficient for now
            if (item.itemLevel == 580)
            {
                return Crafted;
            }

            if (item.itemLevel == 590)
            {
                return Tomes;
            }

            if ((item.itemLevel >= 600) && (item.itemLevel <= 605))
            {
                if (item.name.Contains("augmented", StringComparison.OrdinalIgnoreCase))
                {
                    switch(item.slotCategory)
                    {
                        case 13:
                        case 1:
                        case 4:
                        case 8:
                        case 5:
                        case 3:
                        case 7:
                            return P3S;
                        case 9:
                        case 12:
                        case 10:
                        case 11:
                            return P2S;
                        default:
                            return Unknown;
                    }
                }
                else
                {
                    switch(item.slotCategory)
                    {
                        case 4:
                        case 13:
                        case 1:
                            return P4S;
                        case 7:
                            return P3S;
                        case 8:
                        case 5:
                        case 3:
                            return P2S;
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                            return P1S;
                        default:
                            return Unknown;

                    }
                }
            }

            return Unknown;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Item 1 is source, item 2 is type</returns>
        public static Tuple<string, string> GetGearSourceAndType(GearItem item)
        {
            // It's a bit hacky to hard code this, but sufficient for now
            if (item.itemLevel == 580)
            {
                return new Tuple<string, string>(Crafted, Unknown);
            }

            if (item.itemLevel == 590)
            {
                return new Tuple<string, string>(Tomes, Unknown);
            }

            if ((item.itemLevel >= 600) && (item.itemLevel <= 605))
            {
                if (item.name.Contains("augmented", StringComparison.OrdinalIgnoreCase))
                {
                    switch (item.slotCategory)
                    {
                        case 13:
                            return new Tuple<string, string>(P3S, "Weapon Augment");
                        case 1:
                        case 4:
                        case 8:
                        case 5:
                        case 3:
                        case 7:
                            return new Tuple<string, string>(P3S, "Armor Augment");
                        case 9:
                        case 12:
                        case 10:
                        case 11:
                            return new Tuple<string, string>(P2S, "Accessory Augment");
                        default:
                            return new Tuple<string, string>(Unknown, Unknown);
                    }
                }
                else
                {
                    switch (item.slotCategory)
                    {
                        case 4:
                        case 13:
                        case 1:
                            return new Tuple<string, string>(P4S, item.slotName);
                        case 7:
                            return new Tuple<string, string>(P3S, item.slotName);
                        case 8:
                        case 5:
                        case 3:
                            return new Tuple<string, string>(P2S, item.slotName);
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                            return new Tuple<string, string>(P1S, item.slotName);
                        default:
                            return new Tuple<string, string>(Unknown, Unknown);

                    }
                }
            }

            return new Tuple<string, string>(Unknown, Unknown);
        }
    }
}
