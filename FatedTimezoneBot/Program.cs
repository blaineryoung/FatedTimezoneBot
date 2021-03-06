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
using Serilog;
using FatedTimezoneBot.Logic.Services;
using FatedTimezoneBot.Logic.Services.Stats;
using FatedTimezoneBot.Logic.Stores;
using FatedTimezoneBot.Logic.Stores.FileStores;
using FatedTimezoneBot.Logic.Dispatcher.Listeners;

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
            ILogger logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

            FatedTimezoneBot.Logic.Discord.IDiscordClientWrapper client = new DiscordClient(token, logger);
            await client.Connect();

            MessageDispatcher md = new MessageDispatcher(client, logger);

            IChannelInformationFetcher fetcher = new ChannelFileInformationFetcher(logger);
            ICharacterInformationFetcher characterFetcher = new CharacterRestInformationFetcher(logger);
            IGearInformationFetcher gf = new GearFileInformationFetcher(logger);
            IGearSlotMapperFactory gearSlotMapper = new GearSlotMapperFactory(gf, logger);
            IGearSetInformationFetcher gearSetInformationFetcher = new GearSetRestInformationFetcher(logger);

            IStatsStore statsStore = new FileStatsStore(logger);
            IStatsService statsService = new StatsService(statsStore, logger);

            IEventDispatcher eventDispatcher = new EventDispatcher(client.Client, logger);
            await eventDispatcher.RegisterEvent(new UpdateStatisticsEvent(fetcher, characterFetcher, statsService, logger));

            md.AddHandler(new ConvertTimeCommand(fetcher, logger));
            md.AddHandler(new RaidTimesCommand(fetcher, logger));
            md.AddHandler(new CustomResponseCommand(fetcher, logger));
            md.AddHandler(new ShowChannelStatsCommand(statsService, logger));
            md.AddHandler(new ShowCharacterStatsCommand(fetcher, statsService, logger));
            md.AddHandler(new ShowNeededGearCommand(fetcher, characterFetcher, gf, gearSlotMapper, gearSetInformationFetcher, logger));
            md.AddHandler(new ShowGearSummaryCommand(fetcher, characterFetcher, gf, gearSlotMapper, gearSetInformationFetcher, logger));
            md.AddHandler(new ResetStatsCommand(fetcher, characterFetcher, statsService, logger));

            md.AddListener(new StatsListener(statsService, logger));

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
	}
}
