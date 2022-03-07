using Discord;
using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Serializers;
using FatedTimezoneBot.Logic.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        const string NeededGearCommandString = "!gearsummary";

        ICollection<TimeZoneInfo> timeZones;

        public ShowGearSummaryCommand(
            IChannelInformationFetcher channelInformationFetcher,
            ICharacterInformationFetcher characterInformationFetcher,
            IGearInformationFetcher gearInformationFetcher,
            IGearSlotMapperFactory gearSlotMapperFactory,
            IGearSetInformationFetcher gearSetInformationFetcher)
        {
            this.timeZones = TimeZoneInfo.GetSystemTimeZones();
            this.channelInformationFetcher = channelInformationFetcher;
            this.characterFetcher = characterInformationFetcher;
            this.gearInformationFetcher = gearInformationFetcher;
            this.gearSlotMapper = gearSlotMapperFactory;
            this.gearSetInformationFetcher = gearSetInformationFetcher;
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
                Console.WriteLine($"Got exception {e.Message} when attempting to get channel information for {message.Channel.Id}");
                Console.WriteLine(e.StackTrace);

                return false;
            }

            List<ChannelPlayer> players = new List<ChannelPlayer>();

            // determine who we should retrieve
            players.AddRange(channelInformation.PlayerInformation.PlayerMap.Values);
            ConcurrentBag<Tuple<string, IEnumerable<Tuple<string, string>>>> outputs = new ConcurrentBag<Tuple<string, IEnumerable<Tuple<string, string>>>>();

            var options = new ParallelOptions { MaxDegreeOfParallelism = 8 };
            await Parallel.ForEachAsync(players, options, async (player, token) =>
            {
                foreach (ChannelCharacter character in player.characters)
                {                    
                    outputs.Add(await BuildDiffForCharacter(player.displayname, character));
                }
            });

            // [source][slot] - list
            Dictionary<string, Dictionary<string, StringBuilder>> outputAccumulator = new Dictionary<string, Dictionary<string, StringBuilder>>();

            foreach (Tuple<string, IEnumerable<Tuple<string, string>>> missingThing in outputs)
            {
                foreach (Tuple<string, string> thing in missingThing.Item2)
                {
                    Dictionary<string, StringBuilder> raidComponent;
                    if (false == outputAccumulator.TryGetValue(thing.Item1, out raidComponent))
                    {
                        raidComponent = new Dictionary<string, StringBuilder>();
                        raidComponent.Add(thing.Item2, new StringBuilder(missingThing.Item1));
                        outputAccumulator.Add(thing.Item1, raidComponent);
                    }
                    else
                    {
                        StringBuilder needList = null;
                        if (false == raidComponent.TryGetValue(thing.Item2, out needList))
                        {
                            raidComponent.Add(thing.Item2, new StringBuilder(missingThing.Item1));
                        }
                        else
                        {
                            needList.Append($", {missingThing.Item1}");
                        }
                    }
                }    
            }

            StringBuilder commandOutput = new StringBuilder();
            // That was ugly, now build the actual output
            foreach (KeyValuePair<string, Dictionary<string, StringBuilder>> raidFloor in outputAccumulator.OrderBy(x => x.Key))
            {
                commandOutput.AppendLine($"__{raidFloor.Key}__");
                foreach (KeyValuePair<string, StringBuilder> raidFloorItem in raidFloor.Value)
                {
                    commandOutput.AppendLine($"**{raidFloorItem.Key}**S: {raidFloorItem.Value}");
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

            CharacterInfo characterInfo = await this.characterFetcher.GetCharacterInformation(character.characterid);
            GearSetInfo setInfo = await this.gearSetInformationFetcher.GetGearSetInformation(new Guid(character.bisid));

            GearSlotMap equippedGear = await this.gearSlotMapper.CreateGearSlotMap(characterInfo);
            GearSlotMap bisGear = await this.gearSlotMapper.CreateGearSlotMap(setInfo);

            GearSlotMap missingGear = GearSlotMap.GenerateDiff(equippedGear, bisGear);

            List<Tuple<string, string>> missingItems = new List<Tuple<string, string>>();

            foreach (string slot in missingGear.Slots)
            {
                missingItems.Add(GameUtilities.GetGearSourceAndType(missingGear[slot]));
            }

            string displayName = $"{characterInfo.Character.Name} ({playerDisplayName})";

            return new Tuple<string, IEnumerable<Tuple<string, string>>>(displayName, missingItems);
        }
    }
}
