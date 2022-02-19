using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Utility
{
    public static class TimeUtilities
    {
        private static Regex IsTime = new Regex("((1[0-2]|0?[1-9]):?([0-5][0-9])?\\s*?([AaPp][Mm]))");
        private static Regex MaybeTime = new Regex("(at\\s*?(1[0-2]|0?[1-9]):?([0-5][0-9])?)");

        public static bool ContainsTime(string content)
        {
            MatchCollection mc;

            mc = IsTime.Matches(content);
            if (mc.Count == 0)
            {
                mc = MaybeTime.Matches(content);
            }

            return mc.Count != 0;
        }

        public static DateTime GetTimeFromText(string timeString, TimeZoneInfo sourceTimeZone)
        {
            // Determine if there was a match.
            MatchCollection mc = IsTime.Matches(timeString);
            MatchCollection mcMaybe = MaybeTime.Matches(timeString);
            if (mc.Count == 0 && mcMaybe.Count == 0)
            {
                throw new InvalidDataException($"{timeString} is not a valid time");
            }

            // If there are multiple times matched, we just pick the first.  Could do th is in 
            // a loop if we wanted to be robust.
            int userHour;
            int userMinutes;
            string userMeridian;

            if (mc.Count != 0)
            {
                if (mc[0].Groups.Count < 5)
                {
                    throw new InvalidDataException($"{timeString} is not a valid time");
                }

                // Parse out the entered time.
                userHour = int.Parse(mc[0].Groups[2].Value);
                userMinutes = string.IsNullOrEmpty(mc[0].Groups[3].Value) ? 0 : int.Parse(mc[0].Groups[3].Value);
                userMeridian = mc[0].Groups[4].Value;

            }
            else
            {
                userHour = int.Parse(mcMaybe[0].Groups[2].Value);
                userMinutes = string.IsNullOrEmpty(mcMaybe[0].Groups[3].Value) ? 0 : int.Parse(mcMaybe[0].Groups[3].Value);
                userMeridian = "pm";
            }

            // Deal with wierdness around am/pm
            if (0 == string.Compare(userMeridian, "pm", StringComparison.OrdinalIgnoreCase))
            {
                if (userHour != 12)
                {
                    userHour += 12;
                }
            }
            else
            {
                if (userHour == 12)
                {
                    userHour = 0;
                }
            }

            // Bit of a hack to deal with the c# time system.  Time zones/daylight savings time are dependent on date, so just
            // assume today's date.
            DateTime userTimeToday = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, sourceTimeZone);
            DateTime userTime = new DateTime(userTimeToday.Year, userTimeToday.Month, userTimeToday.Day, userHour, userMinutes, 0);

            userTime.AddHours(userHour);
            userTime.AddMinutes(userMinutes);

            return userTime;
        }

        public static StringBuilder PrintUserTimes(DateTime sourceTime, TimeZoneInfo sourceTimeZone, IReadOnlyDictionary<TimeZoneInfo, string> displayMappings)
        {
            StringBuilder sb = new StringBuilder();

            // Cycle through every group and print out the appropriate time for them.
            foreach (KeyValuePair<TimeZoneInfo, string> displayer in displayMappings)
            {
                DateTime local = TimeZoneInfo.ConvertTime(sourceTime, sourceTimeZone, displayer.Key);
                if (local.DayOfWeek == sourceTime.DayOfWeek)
                {
                    sb.AppendLine($"**{displayer.Value}** - {local.ToShortTimeString()}");
                }
                else
                {
                    sb.AppendLine($"**{displayer.Value}** - {local.ToShortTimeString()} ({local.DayOfWeek})");
                }
            }

            return sb;
        }
    }
}
