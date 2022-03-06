using FatedTimezoneBot.Logic.Information.Serializers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information
{
    public class PlayerInformation
    {
        IReadOnlyDictionary<string, TimeZoneInfo> timeZoneMappings;
        IReadOnlyDictionary<TimeZoneInfo, string> displayMappings;
        IReadOnlyDictionary<string, ChannelPlayer> playerMap;

        public IReadOnlyDictionary<string, TimeZoneInfo> TimeZoneMappings => timeZoneMappings;
        public IReadOnlyDictionary<TimeZoneInfo, string> DisplayMappings => displayMappings;

        public IReadOnlyDictionary<string, ChannelPlayer> PlayerMap => playerMap;

        public PlayerInformation(IEnumerable<ChannelPlayer> p)
        {
            ICollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();

            Dictionary<string, TimeZoneInfo> timeZoneMappings = new Dictionary<string, TimeZoneInfo>();
            Dictionary<TimeZoneInfo, string> displayMappings = new Dictionary<TimeZoneInfo, string>();
            Dictionary<string, ChannelPlayer> playerMap = new Dictionary<string, ChannelPlayer>();

            foreach (ChannelPlayer staticMember in p)
            {
                TimeZoneInfo timeZone = timeZones.Where(x => 0 == string.Compare(x.Id, staticMember.timezoneid)).First();
                timeZoneMappings.Add(staticMember.username, timeZone);

                if (displayMappings.ContainsKey(timeZone))
                {
                    string displayString = displayMappings[timeZone];
                    displayString = $"{displayString}, {staticMember.displayname}";
                    displayMappings[timeZone] = displayString;
                }
                else
                {
                    displayMappings.Add(timeZone, staticMember.displayname);
                }

                playerMap.Add(staticMember.username, staticMember);
            }

            this.timeZoneMappings = new ReadOnlyDictionary<string, TimeZoneInfo>(timeZoneMappings);
            this.displayMappings = new ReadOnlyDictionary<TimeZoneInfo, string>(displayMappings);
            this.playerMap = new ReadOnlyDictionary<string, ChannelPlayer>(playerMap);
        }
    }
}
