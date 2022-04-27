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
using FatedTimezoneBot.Logic.Information.FileFetchers;
using FatedTimezoneBot.Logic.Information.RestFetchers;
using FatedTimezoneBot.Logic.Dispatcher.Events;

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
            ICharacterInformationFetcher characterFetcher = new CharacterRestInformationFetcher();
            IGearInformationFetcher gf = new GearFileInformationFetcher();
            IGearSlotMapperFactory gearSlotMapper = new GearSlotMapperFactory(gf);
            IGearSetInformationFetcher gearSetInformationFetcher = new GearSetRestInformationFetcher();

            IEventDispatcher eventDispatcher = new EventDispatcher(client.Client);
            eventDispatcher.RegisterEvent(new RefreshCharactersEvent(fetcher, characterFetcher, gearSetInformationFetcher));

            md.AddHandler(new ConvertTimeCommand(fetcher));
            md.AddHandler(new RaidTimesCommand(fetcher));
            md.AddHandler(new CustomResponseCommand(fetcher));
            md.AddHandler(new ShowNeededGearCommand(fetcher, characterFetcher, gf, gearSlotMapper, gearSetInformationFetcher));
            md.AddHandler(new ShowGearSummaryCommand(fetcher, characterFetcher, gf, gearSlotMapper, gearSetInformationFetcher));

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
	}
}
