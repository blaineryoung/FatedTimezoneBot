using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information.Serializers
{

    public class GearSetInfo
    {
        public static GearSetInfo Deserialize(string content)
        {
            return JsonConvert.DeserializeObject<GearSetInfo>(content);
        }

        public string id { get; set; }
        public string jobAbbrev { get; set; }
        public string jobIconPath { get; set; }
        public string clanName { get; set; }
        public bool isOwner { get; set; }
        public string name { get; set; }
        public DateTime lastUpdate { get; set; }
        public int minItemLevel { get; set; }
        public int maxItemLevel { get; set; }
        public int minMateriaTier { get; set; }
        public int maxMateriaTier { get; set; }
        public Materia materia { get; set; }
        public Totalparam[] totalParams { get; set; }
        public object buffs { get; set; }
        public object relics { get; set; }
        public float patch { get; set; }
        public int job { get; set; }
        public int clan { get; set; }
        [SlotAttribute]
        public int weapon { get; set; }
        [SlotAttribute]
        public int head { get; set; }
        [SlotAttribute]
        public int body { get; set; }
        [SlotAttribute]
        public int hands { get; set; }
        [SlotAttribute]
        public int legs { get; set; }
        [SlotAttribute]
        public int feet { get; set; }
        [SlotAttribute]
        public int? offHand { get; set; }
        [SlotAttribute]
        public int ears { get; set; }
        [SlotAttribute]
        public int neck { get; set; }
        [SlotAttribute]
        public int wrists { get; set; }
        [SlotAttribute]
        public int fingerL { get; set; }
        [SlotAttribute]
        public int fingerR { get; set; }
        public int food { get; set; }
        public object medicine { get; set; }
    }

    public class Materia
    {
        public _35215 _35215 { get; set; }
        public _35217 _35217 { get; set; }
        public _35218 _35218 { get; set; }
        public _35219 _35219 { get; set; }
        public _35228 _35228 { get; set; }
        public _35233 _35233 { get; set; }
        public _35256 _35256 { get; set; }
        public _35290 _35290 { get; set; }
        public _35291 _35291 { get; set; }
        public _35294 _35294 { get; set; }
        public _35303 _35303 { get; set; }
        public _35308 _35308 { get; set; }
        public _35313 _35313 { get; set; }
        public _35093R _35093R { get; set; }
        public _35243R _35243R { get; set; }
        public _35318L _35318L { get; set; }
    }

    public class _35215
    {
        public int _1 { get; set; }
        public int _2 { get; set; }
    }

    public class _35217
    {
        public int _1 { get; set; }
        public int _2 { get; set; }
    }

    public class _35218
    {
        public int _1 { get; set; }
        public int _2 { get; set; }
    }

    public class _35219
    {
        public int _1 { get; set; }
        public int _2 { get; set; }
    }

    public class _35228
    {
        public int _1 { get; set; }
        public int _2 { get; set; }
    }

    public class _35233
    {
        public int _1 { get; set; }
        public int _2 { get; set; }
    }

    public class _35256
    {
        public int _1 { get; set; }
        public int _2 { get; set; }
    }

    public class _35290
    {
        public int _1 { get; set; }
        public int _2 { get; set; }
    }

    public class _35291
    {
        public int _1 { get; set; }
        public int _2 { get; set; }
    }

    public class _35294
    {
        public int _1 { get; set; }
        public int _2 { get; set; }
    }

    public class _35303
    {
        public int _1 { get; set; }
        public int _2 { get; set; }
    }

    public class _35308
    {
        public int _1 { get; set; }
        public int _2 { get; set; }
    }

    public class _35313
    {
        public int _1 { get; set; }
        public int _2 { get; set; }
    }

    public class _35093R
    {
        public int _1 { get; set; }
        public int _2 { get; set; }
        public int _3 { get; set; }
        public int _4 { get; set; }
        public int _5 { get; set; }
    }

    public class _35243R
    {
        public int _1 { get; set; }
        public int _2 { get; set; }
    }

    public class _35318L
    {
        public int _1 { get; set; }
        public int _2 { get; set; }
    }

    public class Totalparam
    {
        public object id { get; set; }
        public string name { get; set; }
        public float value { get; set; }
        public string units { get; set; }
    }

}
