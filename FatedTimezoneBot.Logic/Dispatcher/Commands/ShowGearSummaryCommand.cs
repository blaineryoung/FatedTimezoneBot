using Discord;
using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Exceptions;
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
    public class ShowGearSummaryCommand : ICommandHandler
    {
        private IChannelInformationFetcher channelInformationFetcher;
        private ICharacterInformationFetcher characterFetcher;
        private IGearInformationFetcher gearInformationFetcher;
        private IGearSlotMapperFactory gearSlotMapper;
        private IGearSetInformationFetcher gearSetInformationFetcher;
        private ILogger _logger;

        const string NeededGearCommandString = "!gearsummary";

        ICollection<TimeZoneInfo> timeZones;

        public ShowGearSummaryCommand(
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
            players.AddRange(channelInformation.PlayerInformation.PlayerMap.Values);
            ConcurrentBag<Tuple<string, IEnumerable<Tuple<string, string>>>> outputs = new ConcurrentBag<Tuple<string, IEnumerable<Tuple<string, string>>>>();

            // Grab the diffs for all players
            var options = new ParallelOptions { MaxDegreeOfParallelism = 8 };
            await Parallel.ForEachAsync(players, options, async (player, token) =>
            {
                foreach (ChannelCharacter character in player.characters)
                {
                    try
                    {
                        outputs.Add(await BuildDiffForCharacter(player.displayname, character));
                    }
                    catch (CharacterNotFoundException) { }
                }
            });

            // [source][slot] - list
            Dictionary<string, Dictionary<string, HashSet<string>>> outputAccumulator = new Dictionary<string, Dictionary<string, HashSet<string>>>();

            foreach (Tuple<string, IEnumerable<Tuple<string, string>>> missingThing in outputs)
            {
                foreach (Tuple<string, string> thing in missingThing.Item2)
                {
                    Dictionary<string, HashSet<string>> raidComponent;
                    if (false == outputAccumulator.TryGetValue(thing.Item1, out raidComponent))
                    {
                        raidComponent = new Dictionary<string, HashSet<string>>();
                        HashSet<string> needList = new HashSet<string>();
                        needList.Add(missingThing.Item1);

                        raidComponent.Add(thing.Item2, needList);
                        outputAccumulator.Add(thing.Item1, raidComponent);
                    }
                    else
                    {
                        HashSet<string> needList = null;
                        if (false == raidComponent.TryGetValue(thing.Item2, out needList))
                        {
                            needList = new HashSet<string>();
                            needList.Add(missingThing.Item1);
                            raidComponent.Add(thing.Item2, needList);
                        }
                        else
                        {
                            needList.Add(missingThing.Item1);
                        }
                    }
                }    
            }

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            StringBuilder commandOutput = new StringBuilder();
            // That was ugly, now build the actual output
            foreach (KeyValuePair<string, Dictionary<string, HashSet<string>>> raidFloor in outputAccumulator.OrderBy(x => x.Key))
            {
                commandOutput.AppendLine($"__{raidFloor.Key}__");
                foreach (KeyValuePair<string, HashSet<string>> raidFloorItem in raidFloor.Value)
                {
                    string needList = string.Join(',', raidFloorItem.Value);
                    commandOutput.AppendLine($"**{textInfo.ToTitleCase(raidFloorItem.Key)}**: {needList}");
                }

                commandOutput.AppendLine();
            }

            EmbedBuilder eb = new EmbedBuilder();


            eb.Description = commandOutput.ToString();
            await message.Channel.SendMessageAsync("", false, eb.Build());

            return true;
        }

        private async Task<Tuple<string, IEnumerable<Tuple<string, string>>>> BuildDiffForCharacter(string playerDisplayName, ChannelCharacter character)
        {
            StringBuilder output = new StringBuilder();
    
            GearSetInfo setInfo = await this.gearSetInformationFetcher.GetGearSetInformation(new Guid(character.bisid));
            CharacterInfo characterInfo = await this.characterFetcher.GetCharacterInformation(character.characterid, setInfo.job);

            GearSlotMap equippedGear = await this.gearSlotMapper.CreateGearSlotMap(characterInfo);
            GearSlotMap bisGear = await this.gearSlotMapper.CreateGearSlotMap(setInfo);

            GearSlotMap missingGear = GearSlotMap.GenerateDiff(equippedGear, bisGear);

            List<Tuple<string, string>> missingItems = new List<Tuple<string, string>>();

            foreach (GearItem slot in missingGear.Gear)
            {
                missingItems.Add(GameUtilities.GetGearSourceAndType(slot));
            }

            string displayName = $"{characterInfo.Character.Name} ({playerDisplayName})";

            return new Tuple<string, IEnumerable<Tuple<string, string>>>(displayName, missingItems);
        }
    }
}
