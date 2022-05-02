using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Services.Stats
{
    public class PlayerStats
    {
        public string PlayerName { get; set; }

        public string PlayerId { get; set; }

        public int MessageCount { get; set; }
        
        public int MountCount { get; set; }

        public int WordCount { get; set; }
    }
}
