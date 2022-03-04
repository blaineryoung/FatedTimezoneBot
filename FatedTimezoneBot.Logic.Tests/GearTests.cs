using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Serializers;

namespace FatedTimezoneBot.Logic.Tests
{
    public class GearTests
    {
        [Test]
        public async Task GetGearTest()
        {
            IGearInformationFetcher gf = new GearFileInformationFetcher();

            GearInformation gearInformation = await gf.GetGearInformation();

            GearItem gi = gearInformation.GearMap[35271];

            Assert.AreEqual(gi.name, "Asphodelos Himation of Maiming");
        }
    }
}
