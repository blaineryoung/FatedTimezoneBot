using FatedTimezoneBot.Logic.Information.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information
{
    public interface ICharacterInformationFetcher
    {
        /// <summary>
        /// Fetches information about the specified character.
        /// </summary>
        /// <param name="characterId">The lodestone id of the character to retrieve</param>
        /// <param name="classId">Optional class id.  Will not refresh cache if class id does not match.</param>
        /// <returns>CharacterInfo class representing the character</returns>
        Task<CharacterInfo> GetCharacterInformation(int characterId, int classId = 0);
    }
}
