﻿using FatedTimezoneBot.Logic.Information.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatedTimezoneBot.Logic.Information
{
    public interface IGearSetInformationFetcher
    {
        Task<GearSetInfo> GetGearSetInformation(Guid gearSetId);
    }
}
