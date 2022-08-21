using Discord;
using FatedTimezoneBot.Logic.Discord;
using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Serializers;
using FatedTimezoneBot.Logic.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Dispatcher.Commands
{
    public class RaidTimesCommand : ICommandHandler
    {
        private IChannelInformationFetcher channelInformationFetcher;
        const string RaidTimesCommandString = "!raidtime";
        private ILogger<RaidTimesCommand> _logger;

        ICollection<TimeZoneInfo> timeZones;

        public RaidTimesCommand(IChannelInformationFetcher channelInformationFetcher, ILogger<RaidTimesCommand> logger)
        {
            this.timeZones = TimeZoneInfo.GetSystemTimeZones();
            this.channelInformationFetcher = channelInformationFetcher;
            _logger = logger;
        }

        public async Task<bool> HandleCommand(IMessage message)
        {
            if (!message.Content.Contains(RaidTimesCommandString, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            ChannelInformation channelInformation = null;
            try
            {
                channelInformation = await channelInformationFetcher.GetChannelInformation(message.Channel.Id);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Attempting to load information for channel {channel}", message.Channel.Id);

                return false;
            }

            return await this.PrintRaidTimes(message, channelInformation);
        }

        private async Task<bool> PrintRaidTimes(IMessage message, ChannelInformation channelInformation)
        {
            StringBuilder raidTimes = new StringBuilder();
            TimeSpan nextRaid = TimeSpan.MaxValue;

            foreach (ChannelRaid r in channelInformation.RaidInfo)
            {
                raidTimes.AppendLine(r.day);

                TimeZoneInfo timeZone = this.timeZones.Where(x => 0 == string.Compare(x.Id, r.timezoneid)).First();
                DateTime raidTime = TimeUtilities.GetTimeFromText(r.time, timeZone);

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

                StringBuilder sb = TimeUtilities.PrintUserTimes(raidTime, timeZone, channelInformation.PlayerInformation.DisplayMappings);

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
                else if (0 != nextRaid.Minutes)
                {
                    timeTillRaid = $"{nextRaid.Minutes} minutes";
                }
                else
                {
                    timeTillRaid = "NOW!";
                }
                raidTimes.AppendLine($"Next raid is in **{timeTillRaid}**");
            }

            EmbedBuilder eb = new EmbedBuilder();

            eb.Description = raidTimes.ToString();
            await message.Channel.SendMessageAsync("", false, eb.Build());

            return true;
        }
    }
}
