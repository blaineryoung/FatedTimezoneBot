using Discord;
using FatedTimezoneBot.Logic.Services;
using FatedTimezoneBot.Logic.Services.Stats;
using FatedTimezoneBot.Logic.Stores;
using Moq;
using NUnit.Framework;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Tests
{
    internal class StatsTests
    {
        private IStatsStore statsStore;
        Mock<IStatsStore> statsMock;
        ILogger logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        const int channelId = 1;

        [SetUp]
        public void Setup()
        {
            ChannelStats stats = new ChannelStats();
            stats.StatStart = DateTime.MinValue;
            stats.ChannelId = channelId;

            PlayerStats player1 = new PlayerStats()
            {
                MessageCount = 5,
                MountCount = 3,
                PlayerId = "FooBar#1234",
                PlayerName = "FooBar",
                WordCount = 10000
            };
            PlayerStats player2 = new PlayerStats()
            {
                MessageCount = 100,
                MountCount = 7,
                PlayerId = "FooBar2#1234",
                PlayerName = "FooBar2",
                WordCount = 9995
            };
            PlayerStats player3 = new PlayerStats()
            {
                MessageCount = 50,
                MountCount = 2,
                PlayerId = "FooBar3#1234",
                PlayerName = "FooBar3",
                WordCount = 10
            };

            stats.PlayerStats.TryAdd(player1.PlayerId, player1);
            stats.PlayerStats.TryAdd(player2.PlayerId, player2);
            stats.PlayerStats.TryAdd(player3.PlayerId, player3);

            statsMock = new Mock<IStatsStore>();
            statsMock.Setup(x => x.GetStatsForChannel(channelId)).ReturnsAsync(stats);
            statsStore = statsMock.Object;
        }

        [Test]
        public async Task BasicStatsTest()
        {
            IStatsService statsService = new StatsService(statsStore, logger);
            string output = await statsService.PrintChannelStats(channelId);

            OutputStringBuilder b = new OutputStringBuilder();

            string expected = b.GetExpectedOutputString();

            Assert.AreEqual(expected, output);
        }

        [Test]
        public async Task SendMessageTest()
        {
            string message = "The quick brown fox jumped over the lazy dog"; // 9 words
            string messageSender = "FooBar2";
            string messageSenderId = "FooBar2#1234";

            IStatsService statsService = new StatsService(statsStore, logger);
            PlayerStats playerStats = await statsService.GetUserStats(channelId, messageSenderId);

            OutputStringBuilder b = new OutputStringBuilder();
            b.CurrentMostWordsUser = messageSender;
            b.CurrentMostWords = playerStats.WordCount + 9;
            b.CurrentMostMessages = playerStats.MessageCount + 1;
            b.CurrentMessages += 1;
            b.CurrentWords += 9;

            IMessage m = DiscordTestUtilities.BuildMessage(channelId, messageSender, message);
            await statsService.ProcessMessage(m);

            string output = await statsService.PrintChannelStats(channelId);
            string expected = b.GetExpectedOutputString();

            Assert.AreEqual(expected, output);
        }

        [Test]
        public async Task MultipleSendMessageTest()
        {
            string message = "The quick brown fox jumped over the lazy dog"; // 9 words
            string messageSender = "FooBar2";
            string messageSenderId = "FooBar2#1234";
            OutputStringBuilder b = new OutputStringBuilder();

            IStatsService statsService = new StatsService(statsStore, logger);
            PlayerStats playerStats = await statsService.GetUserStats(channelId, messageSenderId);

            for (int i = 0; i < 10; i++)
            {
                b.CurrentMostWordsUser = messageSender;
                b.CurrentMostWords = playerStats.WordCount + 9;
                b.CurrentMostMessages = playerStats.MessageCount + 1;
                b.CurrentMessages += 1;
                b.CurrentWords += 9;

                IMessage m = DiscordTestUtilities.BuildMessage(channelId, messageSender, message);
                await statsService.ProcessMessage(m);
            }

            string output = await statsService.PrintChannelStats(channelId);
            string expected = b.GetExpectedOutputString();

            Assert.AreEqual(expected, output);
        }

        [Test]
        public async Task ConcurrentMultipleSendMessageTest()
        {
            string message = "The quick brown fox jumped over the lazy dog"; // 9 words
            string messageSender = "FooBar2";
            string messageSenderId = "FooBar2#1234";
            OutputStringBuilder b = new OutputStringBuilder();

            IStatsService statsService = new StatsService(statsStore, logger);
            PlayerStats playerStats = await statsService.GetUserStats(channelId, messageSenderId);

            int loops = 20;

            b.CurrentMostWordsUser = messageSender;
            b.CurrentMostWords = playerStats.WordCount + (9 * loops);
            b.CurrentMostMessages = playerStats.MessageCount + (1 * loops);
            b.CurrentMessages += (1 * loops);
            b.CurrentWords += (9 * loops);

            Parallel.For(0, loops, async i => 
            {
                IMessage m = DiscordTestUtilities.BuildMessage(channelId, messageSender, message);
                await statsService.ProcessMessage(m);
            });

            string output = await statsService.PrintChannelStats(channelId);
            string expected = b.GetExpectedOutputString();

            Assert.AreEqual(expected, output);
        }
    }

    internal class OutputStringBuilder
    {
        internal const string outputFormat = "Stats as of Monday, January 1, 0001 12:00:00 AM (UTC)\r\nMessages: **{0}**.  Most: {1}: {2}\r\nWords: **{3}**.  Most: {4}: {5}\r\nMounts: **{6}**.  Most: {7}: {8}\r\n";
        internal const int defaultMessages = 155;
        internal const int defaultMostMessages = 100;
        internal const string defaultMostMessagesUser = "FooBar2";
        internal const int defaultWords = 20005;
        internal const int defaultMostWords = 10000;
        internal const string defaultMostWordsUser = "FooBar";
        internal const int defaultMounts = 12;
        internal const int defaultMostMounts = 7;
        internal const string defaultMostMountsUser = "FooBar2";



        public int CurrentMessages { get; set; }
        public int CurrentMostMessages { get; set; }
        public string CurrentMostMessagesUser { get; set; }
        public int CurrentWords { get; set; }
        public int CurrentMostWords { get; set; }
        public string CurrentMostWordsUser { get; set; }

        public int CurrentMounts { get; set; }
        public string CurrentMostMountsUser { get; set; }
        public int CurrentMostMounts { get; set; }

        internal OutputStringBuilder()
        {
            CurrentMessages = defaultMessages;
            CurrentMostMessages = defaultMostMessages;
            CurrentMostMessagesUser = defaultMostMessagesUser;

            CurrentWords = defaultWords;
            CurrentMostWords = defaultMostWords;
            CurrentMostWordsUser = defaultMostWordsUser;

            CurrentMounts = defaultMounts;
            CurrentMostMountsUser = defaultMostMountsUser;
            CurrentMostMounts = defaultMostMounts;
        }

        public string GetExpectedOutputString()
        {
            return string.Format(outputFormat, CurrentMessages, CurrentMostMessagesUser, CurrentMostMessages, CurrentWords, CurrentMostWordsUser, CurrentMostWords, CurrentMounts, CurrentMostMountsUser, CurrentMostMounts);
        }
    }
}
