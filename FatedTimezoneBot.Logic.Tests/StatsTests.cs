using Discord;
using FatedTimezoneBot.Logic.Information.Serializers;
using FatedTimezoneBot.Logic.Services;
using FatedTimezoneBot.Logic.Services.Stats;
using FatedTimezoneBot.Logic.Stores;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Tests
{
    internal class StatsTests
    {
        private IStatsStore statsStore;
        Mock<IStatsStore> statsMock;
        ILogger logger = DiscordTestUtilities.GetLogger();

        const int channelId = 1;
        const int channelId2 = 2;

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

            stats.PlayerStatsCache.TryAdd(player1.PlayerId, player1);
            stats.PlayerStatsCache.TryAdd(player2.PlayerId, player2);
            stats.PlayerStatsCache.TryAdd(player3.PlayerId, player3);

            statsMock = new Mock<IStatsStore>();
            statsMock.Setup(x => x.GetStatsForChannel(channelId)).ReturnsAsync(stats);

            ChannelStats stats2 = new ChannelStats();
            stats.StatStart = DateTime.MinValue;
            stats.ChannelId = channelId2;
            statsMock.Setup(x => x.GetStatsForChannel(channelId2)).ReturnsAsync(stats2);

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
        public async Task EmptyStatsTest()
        {
            IStatsService statsService = new StatsService(statsStore, logger);
            string output = await statsService.PrintChannelStats(channelId2);

            Assert.IsNotNull(output);
        }

        [Test]
        public async Task BasicCharacterStatsTest()
        {
            IStatsService statsService = new StatsService(statsStore, logger);
            PlayerStats playerStats = await statsService.GetUserStats(channelId, "FooBar2#1234");

            PlayerOutputStringBuilder posb = new PlayerOutputStringBuilder(playerStats);

            string output = await statsService.PrintUserStats(channelId, "FooBar2#1234");
            string expected = posb.GetExpectedOutputString();

            Assert.AreEqual(expected, output);
        }

        [Test]
        public async Task ProcessMessageCharacterStatsTest()
        {
            string message = "The quick brown fox jumped over the lazy dog"; // 9 words
            string messageSender = "FooBar2";

            IStatsService statsService = new StatsService(statsStore, logger);
            PlayerStats playerStats = await statsService.GetUserStats(channelId, "FooBar2#1234");

            PlayerOutputStringBuilder posb = new PlayerOutputStringBuilder(playerStats);

            IMessage m = DiscordTestUtilities.BuildMessage(channelId, messageSender, message);
            await statsService.ProcessMessage(m);

            posb.Messages += 1;
            posb.Words += 9;

            string output = await statsService.PrintUserStats(channelId, "FooBar2#1234");
            string expected = posb.GetExpectedOutputString();

            Assert.AreEqual(expected, output);
        }

        [Test]
        public async Task CharacterDoesNotExistGetStstsTest()
        {
            IStatsService statsService = new StatsService(statsStore, logger);

            string output = await statsService.PrintUserStats(channelId2, "FooBar2#1234");

            Assert.IsNotNull(output);
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

        [Test]
        public async Task UpdateCharacterInfoTest()
        {
            CharacterInfo ci = new CharacterInfo();
            OutputStringBuilder b = new OutputStringBuilder();

            string messageSender = "FooBar2";
            string messageSenderId = "FooBar2#1234";

            ci.Mounts = new Mount[200];
            for (int i = 0; i < ci.Mounts.Length; i++)
            {
                ci.Mounts[i] = new Mount();
                ci.Mounts[i].Name = "FooMount";
            }

            ci.Minions = new Minion[200];
            for (int i = 0; i < ci.Mounts.Length; i++)
            {
                ci.Minions[i] = new Minion();
                ci.Minions[i].Name = "FooMount";
            }

            IStatsService statsService = new StatsService(statsStore, logger);

            b.CurrentMostMounts = ci.Mounts.Length;
            b.CurrentMostMountsUser = messageSender;
            b.CurrentMounts = 205;

            await statsService.UpdateCharacterInfo(channelId, messageSenderId, messageSender, ci);

            string output = await statsService.PrintChannelStats(channelId);
            string expected = b.GetExpectedOutputString();

            Assert.AreEqual(expected, output);
        }

        [Test]
        public async Task UpdateCharacterInfoAsyncTest()
        {
            CharacterInfo ci = new CharacterInfo();
            OutputStringBuilder b = new OutputStringBuilder();

            string messageSender = "FooBar2";
            string messageSenderId = "FooBar2#1234";

            ci.Mounts = new Mount[200];
            for (int i = 0; i < ci.Mounts.Length; i++)
            {
                ci.Mounts[i] = new Mount();
                ci.Mounts[i].Name = "FooMount";
            }

            ci.Minions = new Minion[200];
            for (int i = 0; i < ci.Mounts.Length; i++)
            {
                ci.Minions[i] = new Minion();
                ci.Minions[i].Name = "FooMount";
            }

            IStatsService statsService = new StatsService(statsStore, logger);

            b.CurrentMostMounts = ci.Mounts.Length;
            b.CurrentMostMountsUser = messageSender;
            b.CurrentMounts = 205;

            Parallel.For(0, 20, async i =>
            {
                await statsService.UpdateCharacterInfo(channelId, messageSenderId, messageSender, ci);
            });   

            string output = await statsService.PrintChannelStats(channelId);
            string expected = b.GetExpectedOutputString();

            Assert.AreEqual(expected, output);
        }
    }

    internal class PlayerOutputStringBuilder
    {
        internal const string outputFormat = "Messages: **{1}**.\r\nWords: **{2}**.\r\nMounts: **{3}**.\r\n";

        internal int Messages { get; set; }
        internal int Words { get; set; }
        internal int Mounts { get; set; }

        private string characterName;

        internal PlayerOutputStringBuilder(PlayerStats ps)
        {
            this.Messages = ps.MessageCount;
            this.Words = ps.WordCount;
            this.Mounts = ps.MountCount;
            this.characterName = ps.PlayerName;
        }
        public string GetExpectedOutputString()
        {
            return string.Format(outputFormat, characterName, Messages, Words, Mounts);
        }
    }

    internal class OutputStringBuilder
    {
        internal const string outputFormat = "Messages: **{0}**.  Most: {1}: {2}\r\nWords: **{3}**.  Most: {4}: {5}\r\nMounts: **{6}**.  Most: {7}: {8}\r\n";
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
