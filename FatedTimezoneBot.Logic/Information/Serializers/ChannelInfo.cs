﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information.Serializers
{
    public class ChannelInfo
    {
        public string id { get; set; }
        public ChannelRaid[] raids { get; set; }
        public ChannelPlayer[] players { get; set; }
        public static ChannelInfo DeserializeChannelInfo(string channelJson)
        {
            return JsonConvert.DeserializeObject<ChannelInfo>(channelJson);
        }
    }

    public class ChannelRaid
    {
        public string day { get; set; }
        public string timezoneid { get; set; }
        public string time { get; set; }
    }

    public class ChannelPlayer
    {
        public string username { get; set; }
        public string timezoneid { get; set; }
        public string displayname { get; set; }
    }
}