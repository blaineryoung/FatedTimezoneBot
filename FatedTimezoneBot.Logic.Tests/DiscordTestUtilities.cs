using Discord;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Tests
{
    internal class DiscordTestUtilities
    {
        public static IMessage BuildMessage(
            ulong channelId,
            string userName,
            string message)
        {
            Mock<IMessage> messageMock = new Mock<IMessage>();
            Mock<IMessageChannel> messageChannelMock = new Mock<IMessageChannel>();
            messageChannelMock.SetupGet(x => x.Id).Returns(channelId);
            Mock<IUser> userMock = new Mock<IUser>();
            userMock.SetupGet(x => x.IsBot).Returns(false);
            userMock.SetupGet(x => x.Username).Returns(userName);
            userMock.SetupGet(x => x.Discriminator).Returns("1234");

            messageMock.SetupGet(x => x.Author).Returns(userMock.Object);
            messageMock.SetupGet(x => x.Channel).Returns(messageChannelMock.Object);
            messageMock.SetupGet(x => x.Content).Returns(message);

            return messageMock.Object;
        }

        public static ILogger GetLogger()
        {
            var logger = LoggerFactory.Create(config =>
            {
                config.AddConsole();
            }).CreateLogger("Program");

            return logger;
        }
    }
}
