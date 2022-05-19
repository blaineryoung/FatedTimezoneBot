using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information.Serializers
{
    public class CharacterInfo
    {
        public object Achievements { get; set; }
        public object AchievementsPublic { get; set; }
        public Character Character { get; set; }
        public object FreeCompany { get; set; }
        public object FreeCompanyMembers { get; set; }
        public object Friends { get; set; }
        public object FriendsPublic { get; set; }
        public Minion[] Minions { get; set; }
        public Mount[] Mounts { get; set; }
        public object PvPTeam { get; set; }

        public static CharacterInfo Deserialize(string content)
        {
            return JsonConvert.DeserializeObject<CharacterInfo>(content);
        }
    }

    public class Character
    {
        public Activeclassjob ActiveClassJob { get; set; }
        public string Avatar { get; set; }
        public string Bio { get; set; }
        public Classjob[] ClassJobs { get; set; }
        public Classjobselemental ClassJobsElemental { get; set; }
        public string DC { get; set; }
        public string FreeCompanyId { get; set; }
        public string FreeCompanyName { get; set; }
        public Gearset GearSet { get; set; }
        public int Gender { get; set; }
        public Grandcompany GrandCompany { get; set; }
        public int GuardianDeity { get; set; }
        public int ID { get; set; }
        public object Lang { get; set; }
        public string Name { get; set; }
        public string Nameday { get; set; }
        public int ParseDate { get; set; }
        public string Portrait { get; set; }
        public object PvPTeamId { get; set; }
        public int Race { get; set; }
        public string Server { get; set; }
        public int Title { get; set; }
        public bool TitleTop { get; set; }
        public int Town { get; set; }
        public int Tribe { get; set; }
    }

    public class Activeclassjob
    {
        public int ClassID { get; set; }
        public int ExpLevel { get; set; }
        public int ExpLevelMax { get; set; }
        public int ExpLevelTogo { get; set; }
        public bool IsSpecialised { get; set; }
        public int JobID { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public Unlockedstate? UnlockedState { get; set; }
    }

    public class Unlockedstate
    {
        public int? ID { get; set; }
        public string Name { get; set; }
    }

    public class Classjobsbozjan
    {
        public int Level { get; set; }
        public int Mettle { get; set; }
        public string Name { get; set; }
    }

    public class Classjobselemental
    {
        public int ExpLevel { get; set; }
        public int ExpLevelMax { get; set; }
        public int ExpLevelTogo { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
    }

    public class Gearset
    {
        public Attributes Attributes { get; set; }
        public int ClassID { get; set; }
        public Gear Gear { get; set; }
        public string GearKey { get; set; }
        public int JobID { get; set; }
        public int Level { get; set; }
    }

    public class Attributes
    {
        public int _1 { get; set; }
        public int _2 { get; set; }
        public int _3 { get; set; }
        public int _4 { get; set; }
        public int _5 { get; set; }
        public int _6 { get; set; }
        public int _7 { get; set; }
        public int _8 { get; set; }
        public int _19 { get; set; }
        public int _20 { get; set; }
        public int _21 { get; set; }
        public int _22 { get; set; }
        public int _24 { get; set; }
        public int _27 { get; set; }
        public int _33 { get; set; }
        public int _34 { get; set; }
        public int _44 { get; set; }
        public int _45 { get; set; }
        public int _46 { get; set; }
    }

    public class Gear
    {
        public CharacterGearItem Body { get; set; }
        public CharacterGearItem Bracelets { get; set; }
        public CharacterGearItem Earrings { get; set; }
        public CharacterGearItem Feet { get; set; }
        public CharacterGearItem Hands { get; set; }
        public CharacterGearItem Head { get; set; }
        public CharacterGearItem Legs { get; set; }
        public CharacterGearItem MainHand { get; set; }
        public CharacterGearItem Necklace { get; set; }
        public CharacterGearItem Ring1 { get; set; }
        public CharacterGearItem Ring2 { get; set; }
    }

    public class CharacterGearItem
    {
        public object Creator { get; set; }
        public int? Dye { get; set; }
        public int ID { get; set; }
        public int[] Materia { get; set; }
        public int? Mirage { get; set; }
    }

    public class Grandcompany
    {
        public int NameID { get; set; }
        public int RankID { get; set; }
    }

    public class Classjob
    {
        public int ClassID { get; set; }
        public int ExpLevel { get; set; }
        public int ExpLevelMax { get; set; }
        public int ExpLevelTogo { get; set; }
        public bool IsSpecialised { get; set; }
        public int JobID { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public Unlockedstate1? UnlockedState { get; set; }
    }

    public class Unlockedstate1
    {
        public int? ID { get; set; }
        public string Name { get; set; }
    }

    public class Minion
    {
        public string Icon { get; set; }
        public string Name { get; set; }
    }

    public class Mount
    {
        public string Icon { get; set; }
        public string Name { get; set; }
    }

}
