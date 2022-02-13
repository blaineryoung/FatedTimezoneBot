using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;

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

            _client = new DiscordSocketClient();

            _client.Log += Log;

            var token = File.ReadAllText(tokenFile);

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _client.MessageReceived += _client_MessageReceived;
            _client.Ready += () =>
            {
                Console.WriteLine("Bot is connected!");
                return Task.CompletedTask;
            };

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task _client_MessageReceived(SocketMessage message)
        {
            // Short circuit bots
            if (message.Author.IsBot)
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

        private async Task HandleRaid(SocketMessage message)
        {
            StringBuilder raidTimes = new StringBuilder();
            TimeSpan nextRaid = TimeSpan.MaxValue;

            foreach (Raid r in this.raids)
            {
                raidTimes.AppendLine(r.day);
                DateTime raidTime = this.GetTimeFromText(r.time);
                TimeZoneInfo timeZone = TimeZones.Where(x => 0 == string.Compare(x.Id, r.timezoneid)).First();

                DayOfWeek raidDay;
                if (false != Enum.TryParse<DayOfWeek>(r.day, out raidDay))
                {
                    int daysTillRaid = (7 - Math.Abs(raidDay - raidTime.DayOfWeek)) % 7;
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

            EmbedBuilder eb = new EmbedBuilder();

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
                    
            eb.Description = raidTimes.ToString();
            await message.Channel.SendMessageAsync("", false, eb.Build());
        }

        private async Task HandleTime(SocketMessage message)
        {
            string distinctUsername = $"{message.Author.Username}#{message.Author.Discriminator}";

            // Get the time zone for the user
            TimeZoneInfo tz;
            if (false == timeZoneMappings.TryGetValue(distinctUsername, out tz))
            {
                Console.WriteLine($"User {distinctUsername} not found");
                return;
            }

            DateTime userTime = this.GetTimeFromText(message.Content);


            EmbedBuilder eb = new EmbedBuilder();
            StringBuilder sb = this.PrintUserTimes(userTime, tz, this.displayMappings);

            eb.Description = sb.ToString();
            await message.Channel.SendMessageAsync("", false, eb.Build());
        }

        private DateTime GetTimeFromText(string timeString)
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
            DateTime userTimeToday = DateTime.Today;
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
