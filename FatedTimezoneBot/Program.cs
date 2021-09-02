using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text;

namespace FatedTimezoneBot
{
    class Program
    {
		private DiscordSocketClient _client;

        Dictionary<string, TimeZoneInfo> timeZoneMappings = new Dictionary<string, TimeZoneInfo>();
        Dictionary<TimeZoneInfo, string> displayMappings = new Dictionary<TimeZoneInfo, string>();

        Regex IsTime = new Regex("((1[0-2]|0?[1-9]):([0-5][0-9]) ?([AaPp][Mm]))");

		public static void Main(string[] args)
    		=> new Program().MainAsync(args[0], args[1]).GetAwaiter().GetResult();


        public async Task MainAsync(string tokenFile, string playerFile)
		{
            // Set up the mappings of user to time zone.
            ICollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();
            foreach (Player staticMember in await PlayerList.LoadPlayerFile(playerFile))
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

            // Set up the display mappings

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
            if (message.Author.IsBot)
            {
                return;
            }

            MatchCollection mc = IsTime.Matches(message.Content);
            if (mc.Count == 0)
            {
                return;
            }

            if (mc[0].Groups.Count < 5)
            {
                return;
            }

            string distinctUsername = $"{message.Author.Username}#{message.Author.Discriminator}";

            // Get the time zone for the user
            TimeZoneInfo tz;
            if (false == timeZoneMappings.TryGetValue(distinctUsername, out tz))
            {
                Console.WriteLine($"User {distinctUsername} not found");
                return;
            }

            int userHour = int.Parse(mc[0].Groups[2].Value);
            int userMinutes = int.Parse(mc[0].Groups[3].Value);
            string userMeridian = mc[0].Groups[4].Value;

            if (0 == string.Compare(userMeridian, "pm", StringComparison.OrdinalIgnoreCase))
            {
                userHour += 12;
            }

            DateTime userTimeToday = DateTime.Today;
            DateTime userTime = new DateTime(userTimeToday.Year, userTimeToday.Month, userTimeToday.Day, userHour, userMinutes, 0);

            userTime.AddHours(userHour);
            userTime.AddMinutes(userMinutes);

            EmbedBuilder eb = new EmbedBuilder();
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<TimeZoneInfo, string> displayer in this.displayMappings)
            {
                
                DateTime local = TimeZoneInfo.ConvertTime(userTime, tz, displayer.Key);
                sb.AppendLine($"**{displayer.Value}** - {local.ToShortTimeString()}");
            }

            eb.Description = sb.ToString();
            await message.Channel.SendMessageAsync("Local times for everyone", false, eb.Build());
        }

        private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
	}
}
