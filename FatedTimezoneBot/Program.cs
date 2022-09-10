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
using FatedTimezoneBot.Logic.Services;
using FatedTimezoneBot.Logic.Services.Stats;
using FatedTimezoneBot.Logic.Stores;
using FatedTimezoneBot.Logic.Stores.FileStores;
using FatedTimezoneBot.Logic.Dispatcher.Listeners;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using FatedTimezoneBot.Logic.Utility;

namespace FatedTimezoneBot
{
    class Program
    {
        public static void Main(string[] args)
    		=> new Program().MainAsync(args[0]).GetAwaiter().GetResult();


        public async Task MainAsync(string tokenFile)
		{            
            // Do setup stuff.  Normally this would be in the ioc file.

            using var channel = new InMemoryChannel();
            IServiceCollection services = new ServiceCollection();
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.local.json"); //or what ever file you have the settings
            IConfiguration configuration = builder.Build();

            services.Configure<TelemetryConfiguration>(config => config.TelemetryChannel = channel);
            services.AddLogging(builder =>
            {
                // Only Application Insights is registered as a logger provider
                builder.AddApplicationInsights(
                    configureTelemetryConfiguration: (config) => config.ConnectionString = configuration["AppInsightsKey"],
                    configureApplicationInsightsLoggerOptions: (options) => { }
                );
                builder.AddConsole();
            });

            FatedTimezoneBot.Logic.Discord.IDiscordClientWrapper client = new DiscordClient(configuration["DiscordToken"]);
            await client.Connect();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IDiscordClientWrapper>(client);

            services.AddSingleton<IMessageDispatcher, MessageDispatcher>();
            services.AddSingleton<IEventDispatcher, EventDispatcher>();

            services.AddSingleton<IChannelInformationFetcher, ChannelFileInformationFetcher>();
            services.AddSingleton<ICharacterInformationFetcher, CharacterRestInformationFetcher>();
            services.AddSingleton<IGearInformationFetcher, GearFileInformationFetcher>();
            services.AddSingleton<IGearSlotMapperFactory, GearSlotMapperFactory>();
            services.AddSingleton<IGearSetInformationFetcher, GearSetRestInformationFetcher>();
            services.AddSingleton<IStatsStore, FileStatsStore>();
            services.AddSingleton<IStatsService, StatsService>();

            services.AddSingleton<ConvertTimeCommand>();

            IEnumerable<Type> commands = BotUtilities.GetSubclasses(typeof(ICommandHandler));
            foreach (Type type in commands)
            {
                services.AddSingleton(type);
            }

            IEnumerable<Type> events = BotUtilities.GetSubclasses(typeof(IEventHandler));
            foreach (Type type in events)
            {
                services.AddSingleton(type);
            }

            IEnumerable<Type> listeners = BotUtilities.GetSubclasses(typeof(IListener));
            foreach (Type type in listeners)
            {
                services.AddSingleton(type);
            }

            // Register everything
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            IMessageDispatcher md = serviceProvider.GetRequiredService<IMessageDispatcher>();
            IEventDispatcher eventDispatcher = serviceProvider.GetRequiredService<IEventDispatcher>();

            foreach (Type type in commands)
            {
                md.AddHandler(serviceProvider.GetRequiredService(type) as ICommandHandler);
            }

            foreach (Type type in listeners)
            {
                md.AddListener(serviceProvider.GetRequiredService(type) as IListener);
            }

            foreach (Type type in events)
            {
                eventDispatcher.RegisterEvent(serviceProvider.GetRequiredService(type) as IEventHandler);
            }

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
	}
}
