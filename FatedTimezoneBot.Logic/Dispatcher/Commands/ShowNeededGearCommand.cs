using Discord;
using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Serializers;
using FatedTimezoneBot.Logic.Utility;
using System;
using System.Collections.Generic;
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

        public ShowNeededGearCommand(
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

            StringBuilder output = new StringBuilder();

            foreach (ChannelPlayer player in players)
            {
                output.AppendLine($"**------{player.displayname}------**");
                foreach (ChannelCharacter character in player.characters)
                {
                    output.Append(await this.BuildDiffForCharacter(character));
                }
                output.AppendLine();
            }

            EmbedBuilder eb = new EmbedBuilder();

            eb.Description = output.ToString();
            await message.Channel.SendMessageAsync("", false, eb.Build());

            return true;
        }

        private async Task<StringBuilder> BuildDiffForCharacter(ChannelCharacter character)
        {
            StringBuilder output = new StringBuilder();

            CharacterInfo characterInfo = await this.characterFetcher.GetCharacterInformation(character.characterid);
            GearSetInfo setInfo = await this.gearSetInformationFetcher.GetGearSetInformation(new Guid(character.bisid));

            GearSlotMap equippedGear = await this.gearSlotMapper.CreateGearSlotMap(characterInfo);
            GearSlotMap bisGear = await this.gearSlotMapper.CreateGearSlotMap(setInfo);

            GearSlotMap missingGear = GearSlotMap.GenerateDiff(equippedGear, bisGear);

            output.AppendLine($"**{characterInfo.Character.Name}**");
            if (missingGear.Count == 0)
            {
                output.AppendLine("Currently in BIS");
            }
            else
            {
                foreach (string slot in missingGear.Slots)
                {
                    output.AppendLine($"{slot} - {missingGear[slot].name}");
                }
            }

            return output;
        }
    }
}
