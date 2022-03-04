using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information
{
    public interface IGearInformationFetcher
    {
        Task<GearInformation> GetGearInformation();
    }
}
