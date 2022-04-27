using Discord;
using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Serializers;
using FatedTimezoneBot.Logic.Utility;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Dispatcher.Commands
{
    public class ShowNeededGearCommand : ICommandHandler
    {
        private IChannelInformationFetcher channelInformationFetcher;
        private ICharacterInformationFetcher characterFetcher;
        private IGearInformationFetcher gearInformationFetcher;
        private IGearSlotMapperFactory gearSlotMapper;
        private IGearSetInformationFetcher gearSetInformationFetcher;

        const string NeededGearCommandString = "!neededgear";

        ICollection<TimeZoneInfo> timeZones;
        private ILogger _logger;

        public ShowNeededGearCommand(
            IChannelInformationFetcher channelInformationFetcher,
            ICharacterInformationFetcher characterInformationFetcher,
            IGearInformationFetcher gearInformationFetcher,
            IGearSlotMapperFactory gearSlotMapperFactory,
            IGearSetInformationFetcher gearSetInformationFetcher, 
            ILogger logger)
        {
            this.timeZones = TimeZoneInfo.GetSystemTimeZones();
            this.channelInformationFetcher = channelInformationFetcher;
            this.characterFetcher = characterInformationFetcher;
            this.gearInformationFetcher = gearInformationFetcher;
            this.gearSlotMapper = gearSlotMapperFactory;
            this.gearSetInformationFetcher = gearSetInformationFetcher;
            this._logger = logger;
        }

        public async Task<bool> HandleCommand(IMessage message)
        {
            if (!message.Content.Contains(NeededGearCommandString, StringComparison.OrdinalIgnoreCase))
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
                _logger.Warning(e, "Attempting to load information for channel {channel}", message.Channel.Id);

                return false;
            }

            List<ChannelPlayer> players = new List<ChannelPlayer>();

            // determine who we should retrieve
            String[] tokens = message.Content.Split(' ');
            if ((tokens.Length > 1) && (0 == string.Compare(tokens[1], "all", StringComparison.OrdinalIgnoreCase)))
            {
                players.AddRange(channelInformation.PlayerInformation.PlayerMap.Values);
            }
            else
            {
                string playerName;
                if(tokens.Length > 1)
                {
                    playerName = tokens[1];
                }
                else
                {
                    playerName = DiscordUtilities.GetDisambiguatedUser(message.Author);
                }

                ChannelPlayer channelPlayer;
                if (false == channelInformation.PlayerInformation.PlayerMap.TryGetValue(playerName, out channelPlayer))
                {
                    if (false == channelInformation.PlayerInformation.PlayerDisplayNameMap.TryGetValue(playerName, out channelPlayer))
                    {
                        return false;
                    }
                }
                players.Add(channelPlayer);
            }

            ConcurrentBag<StringBuilder> outputs = new ConcurrentBag<StringBuilder>();

            var options = new ParallelOptions { MaxDegreeOfParallelism = 8 };
            await Parallel.ForEachAsync(players, options, async (player, token) =>
            {
                StringBuilder output = new StringBuilder();
                output.AppendLine($"**------{player.displayname}------**");
                foreach (ChannelCharacter character in player.characters)
                {
                    try
                    {
                        output.Append(await this.BuildDiffForCharacter(character));
                    }
                    catch (Exception e)
                    {
                        _logger.Warning(e, "Could not build diff for character {characterID}", character.characterid);
                    }
                }
                output.AppendLine();
                outputs.Add(output);
            });

            EmbedBuilder eb = new EmbedBuilder();

            StringBuilder commandOutput = new StringBuilder();
            foreach (StringBuilder o in outputs)
            {
                commandOutput.AppendLine(o.ToString());
            }

            eb.Description = commandOutput.ToString();
            await message.Channel.SendMessageAsync("", false, eb.Build());

            return true;
        }

        private async Task<StringBuilder> BuildDiffForCharacter(ChannelCharacter character)
        {
            StringBuilder output = new StringBuilder();
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            GearSetInfo setInfo = await this.gearSetInformationFetcher.GetGearSetInformation(new Guid(character.bisid));
            CharacterInfo characterInfo = await this.characterFetcher.GetCharacterInformation(character.characterid, setInfo.job);

            GearSlotMap equippedGear = await this.gearSlotMapper.CreateGearSlotMap(characterInfo);
            GearSlotMap bisGear = await this.gearSlotMapper.CreateGearSlotMap(setInfo);

            GearSlotMap missingGear = GearSlotMap.GenerateDiff(equippedGear, bisGear);

            Dictionary<string, int> sourceCounts = new Dictionary<string, int>();

            output.AppendLine($"**{characterInfo.Character.Name}** (https://etro.gg/gearset/{character.bisid})");
            if (missingGear.Count == 0)
            {
                output.AppendLine("Currently in BIS");
            }
            else
            {
                foreach (GearItem missing in missingGear.Gear)
                {
                    string source = GameUtilities.GetGearSource(missing);

                    int currentCount = 0;
                    if (sourceCounts.ContainsKey(source))
                    {
                        currentCount = sourceCounts[source];
                    }

                    currentCount++;
                    sourceCounts[source] = currentCount;

                    string slotName = textInfo.ToTitleCase(missing.slotName);

                    output.AppendLine($"{slotName} - {missing.name} ({source})");
                }

                output.AppendLine();
                foreach (KeyValuePair<string, int> entry in sourceCounts)
                {
                    output.AppendLine($"**{entry.Key}**-{entry.Value}   ");
                }
                output.AppendLine();
            }

            return output;
        }
    }
}
