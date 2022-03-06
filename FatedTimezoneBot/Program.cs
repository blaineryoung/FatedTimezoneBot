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
using FatedTimezoneBot.Logic.Dispatcher;
using FatedTimezoneBot.Logic.Dispatcher.Commands;

namespace FatedTimezoneBot
{
    class Program
    {
        public static void Main(string[] args)
    		=> new Program().MainAsync(args[0]).GetAwaiter().GetResult();


        public async Task MainAsync(string tokenFile)
		{            
            var token = File.ReadAllText(tokenFile);

            // Do setup stuff.  Normally this would be in the ioc file.
            FatedTimezoneBot.Logic.Discord.IDiscordClientWrapper client = new DiscordClient(token);
            await client.Connect();

            MessageDispatcher md = new MessageDispatcher(client);

            IChannelInformationFetcher fetcher = new ChannelFileInformationFetcher();
            ICharacterInformationFetcher characterFetcher = new CharacterFileInformationFetcher();
            IGearInformationFetcher gf = new GearFileInformationFetcher();
            IGearSlotMapperFactory gearSlotMapper = new GearSlotMapperFactory(gf);
            IGearSetInformationFetcher gearSetInformationFetcher = new GearSetFileInformationFetcher(gf);

            md.AddHandler(new ConvertTimeCommand(fetcher));
            md.AddHandler(new RaidTimesCommand(fetcher));
            md.AddHandler(new ShowNeededGearCommand(fetcher, characterFetcher, gf, gearSlotMapper, gearSetInformationFetcher));

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
	}
}
