using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Utility
{
    public static class DiscordUtilities
    {
        public static string GetDisambiguatedUser(IUser user)
        {
            return $"{user.Username}#{user.Discriminator}";
        }
    }
}
