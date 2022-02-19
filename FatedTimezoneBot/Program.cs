using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using FatedTimezoneBot.Logic.Discord;
using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Serializers;

namespace FatedTimezoneBot
{
    class Program
    {
		private DiscordSocketClient _client;

        ICollection<TimeZoneInfo> TimeZones;
        Dictionary<string, TimeZoneInfo> timeZoneMappings = new Dictionary<string, TimeZoneInfo>();
        Dictionary<TimeZoneInfo, string> displayMappings = new Dictionary<TimeZoneInfo, string>();
        IEnumerable<Raid> raids;

        Regex IsTime = new Regex("((1[0-2]|0?[1-9]):?([0-5][0-9])?\\s*?([AaPp][Mm]))");
        Regex MaybeTime = new Regex("(at\\s*?(1[0-2]|0?[1-9]):?([0-5][0-9])?)");
        const string RaidTimesCommand = "!raidtime";

        public static void Main(string[] args)
    		=> new Program().MainAsync(args[0], args[1], args[2]).GetAwaiter().GetResult();


        public async Task MainAsync(string tokenFile, string playerFile, string raidFile)
		{
            // Set up the mappings of user to time zone.
            TimeZones = TimeZoneInfo.GetSystemTimeZones();
            foreach (Player staticMember in await PlayerList.LoadPlayerFile(playerFile))
            {
                TimeZoneInfo timeZone = TimeZones.Where(x => 0 == string.Compare(x.Id, staticMember.timezoneid)).First();
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

            raids = await RaidInfo.LoadRaidFile(raidFile);


            var token = File.ReadAllText(tokenFile);

            FatedTimezoneBot.Logic.Discord.IDiscordClient client = new DiscordClient(token);
            await client.Connect();

            client.MessageReceived += _client_MessageReceived;

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task _client_MessageReceived(IDiscordMessage message)
        {
            // Short circuit bots
            if (message.IsBot)
            {
                return;
            }

            // Determine if there was a match.
            MatchCollection mc = IsTime.Matches(message.Content);
            try
            {
                if (mc.Count != 0)
                {
                    await HandleTime(message);
                    return;
                }
                if (message.Content.Contains(RaidTimesCommand, StringComparison.OrdinalIgnoreCase))
                {
                    await HandleRaid(message);
                }

                mc = MaybeTime.Matches(message.Content);
                if (mc.Count != 0)
                {
                    await HandleTime(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task HandleRaid(IDiscordMessage message)
        {
            StringBuilder raidTimes = new StringBuilder();
            TimeSpan nextRaid = TimeSpan.MaxValue;

            foreach (Raid r in this.raids)
            {
                raidTimes.AppendLine(r.day);
                
                TimeZoneInfo timeZone = TimeZones.Where(x => 0 == string.Compare(x.Id, r.timezoneid)).First();
                DateTime raidTime = this.GetTimeFromText(r.time, timeZone);

                DayOfWeek raidDay;
                if (false != Enum.TryParse<DayOfWeek>(r.day, out raidDay))
                {
                    int daysTillRaid = raidDay >= raidTime.DayOfWeek ? (raidDay - raidTime.DayOfWeek) : (7 - (int)raidTime.DayOfWeek + (int)raidDay);
                    raidTime = raidTime.AddDays(daysTillRaid);

                    DateTime utcRaidTime = TimeZoneInfo.ConvertTime(raidTime, timeZone, TimeZoneInfo.Utc);

                    TimeSpan ts = utcRaidTime.Subtract(DateTime.UtcNow);

                    if (nextRaid > ts)
                    {
                        nextRaid = ts;
                    }
                }         

                StringBuilder sb = this.PrintUserTimes(raidTime, timeZone, this.displayMappings);

                raidTimes.Append(sb.ToString());
                raidTimes.AppendLine();
            }

            if (nextRaid < TimeSpan.MaxValue)
            {
                String timeTillRaid = String.Empty;
                if (0 != nextRaid.Days)
                {
                    timeTillRaid = $"{nextRaid.Days} days, {nextRaid.Hours} hours, {nextRaid.Minutes} minutes";
                }
                else if (0 != nextRaid.Hours)
                {
                    timeTillRaid = $"{nextRaid.Hours} hours, {nextRaid.Minutes} minutes";
                }
                else if(0 != nextRaid.Minutes)
                {
                    timeTillRaid = $"{nextRaid.Minutes} minutes";
                }
                else
                {
                    timeTillRaid = "NOW!";
                }
                raidTimes.AppendLine($"Next raid is in **{timeTillRaid}**");
            }        

            await message.SendEmbededMessageAsync(raidTimes.ToString());
        }

        private async Task HandleTime(IDiscordMessage message)
        {
            // Get the time zone for the user
            TimeZoneInfo tz;
            if (false == timeZoneMappings.TryGetValue(message.UserName, out tz))
            {
                Console.WriteLine($"User {message.UserName} not found");
                return;
            }

            DateTime userTime = this.GetTimeFromText(message.Content, tz);

            StringBuilder sb = this.PrintUserTimes(userTime, tz, this.displayMappings);

            await message.SendEmbededMessageAsync(sb.ToString());
        }

        private DateTime GetTimeFromText(string timeString, TimeZoneInfo sourceTimeZone)
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

        private StringBuilder PrintUserTimes(DateTime sourceTime, TimeZoneInfo sourceTimeZone, Dictionary<TimeZoneInfo, string> displayMappings)
        {
            StringBuilder sb = new StringBuilder();

            // Cycle through every group and print out the appropriate time for them.
            foreach (KeyValuePair<TimeZoneInfo, string> displayer in this.displayMappings)
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

        private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
	}
}
