﻿using FatedTimezoneBot.Logic.Information.Serializers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information.FileFetchers
{
    public class CharacterFileInformationFetcher : ICharacterInformationFetcher
    {
        // This is a cheesy in memory cache.  If this ever gets big, we'll need to do something better. 
        // It also doesn't handle file changes.  Simple thing to do would be add a file system watcher.  Later
        private ConcurrentDictionary<int, CharacterInfo> characterCache = new ConcurrentDictionary<int, CharacterInfo>();


        public async Task<CharacterInfo> GetCharacterInformation(int characterId, int classId = 0)
        {

            CharacterInfo characterInfo = null;
            if (false == characterCache.TryGetValue(characterId, out characterInfo))
            {
                string fileName = $"gamedata\\characters\\{characterId}.json";
                string content;
                using (StreamReader sr = new StreamReader(fileName))
                {
                    content = await sr.ReadToEndAsync();
                }

                characterInfo = CharacterInfo.Deserialize(content);

                characterCache.TryAdd(characterId, characterInfo);
            }

            return characterInfo;
        }
        
    }
}
