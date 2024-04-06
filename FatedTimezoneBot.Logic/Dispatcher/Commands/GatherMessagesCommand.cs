using Discord;
using FatedTimezoneBot.Logic.Discord;
using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Exceptions;
using FatedTimezoneBot.Logic.Information.Serializers;
using FatedTimezoneBot.Logic.Services;
using FatedTimezoneBot.Logic.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Dispatcher.Commands
{
    public class GatherMessagesCommand : ICommandHandler
    {
        const string ShowChannelStatsCommandString = "!getmessages";

        private ILogger<GatherMessagesCommand> _logger;

        public GatherMessagesCommand(
            ILogger<GatherMessagesCommand> logger)
        {
            _logger = logger;
        }

        public async Task<bool> HandleCommand(IMessage message)
        {
            if (!message.Content.Contains(ShowChannelStatsCommandString, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (0 != string.Compare(DiscordUtilities.GetDisambiguatedUser(message.Author), "DaktCole#0000", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("{baduser} tried to reset stats.  That's bad and not Dakt", DiscordUtilities.GetDisambiguatedUser(message.Author));
                return false;
            }

            await message.Channel.SendMessageAsync("Gathering messages", false);
            using (StreamWriter writer = new StreamWriter("d:\\data\\logs\\messages.txt"))
            {
                bool moreMessages = true;
                ulong currentPointer = message.Id;
                int currentCount = 0;
                while (moreMessages)
                {
                    moreMessages = false;
                    var messages = await message.Channel.GetMessagesAsync(currentPointer, Direction.Before).FlattenAsync<IMessage>();
                    foreach (var m in messages.OrderByDescending(x => x.Id))
                    {
                        await writer.WriteLineAsync(m.Content);
                        moreMessages = true;
                        currentPointer = m.Id;
                        currentCount++;
                    }
                    _logger.LogInformation("Processed {count} messages for channel {channelId}", currentCount, message.Channel.Id);
                }
            }
            _logger.LogInformation("Refresh complete");
            await message.Channel.SendMessageAsync("Rebuild complete", false);

            return true;
        }

    }
}
