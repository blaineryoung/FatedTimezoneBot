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

        public IReadOnlyDictionary<string, TimeZoneInfo> TimeZoneMappings => timeZoneMappings;
        public IReadOnlyDictionary<TimeZoneInfo, string> DisplayMappings => displayMappings;

        public PlayerInformation(IEnumerable<ChannelPlayer> p)
        {
            ICollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();

            Dictionary<string, TimeZoneInfo> timeZoneMappings = new Dictionary<string, TimeZoneInfo>();
            Dictionary<TimeZoneInfo, string> displayMappings = new Dictionary<TimeZoneInfo, string>();

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
            }

            this.timeZoneMappings = new ReadOnlyDictionary<string, TimeZoneInfo>(timeZoneMappings);
            this.displayMappings = new ReadOnlyDictionary<TimeZoneInfo, string>(displayMappings);
        }
    }
}
