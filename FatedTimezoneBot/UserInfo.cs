using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot
{
     public class PlayerList
    {
        public Player[] players { get; set; }

        public static async Task<IEnumerable<Player>> LoadPlayerFile(string playerFile)
        {
            string content;
            using (StreamReader sr = new StreamReader(playerFile))
            {
                content = await sr.ReadToEndAsync();
            }

            PlayerList list = JsonConvert.DeserializeObject<PlayerList>(content);
            return list.players.AsEnumerable();
        }
    }

    public class Player
    {
        public string username { get; set; }
        public string timezoneid { get; set; }
        public string displayname { get; set; }
    }



}
