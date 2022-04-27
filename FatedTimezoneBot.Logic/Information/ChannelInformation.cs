using FatedTimezoneBot.Logic.Information.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information
{
    public class ChannelInformation
    {
        private ChannelInfo channelInfo;
        private PlayerInformation playerInformation;

        public PlayerInformation PlayerInformation => playerInformation;

        public IEnumerable<ChannelRaid> RaidInfo => channelInfo.raids;

        public IEnumerable<ChannelResponse> CustomResponse => channelInfo.responses; 

        public ulong ChannelId { get; }

        internal ChannelInformation(ChannelInfo ci)
        {
            this.playerInformation = new PlayerInformation(ci.players);
            this.channelInfo = ci;
        }
    }
}
