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

        [Test]
        public async Task GetGearSetTest()
        {
            IGearInformationFetcher gf = new GearFileInformationFetcher();
            IGearSetInformationFetcher gearSetInformationFetcher = new GearSetFileInformationFetcher(gf);

            GearSetInformation gsi = await gearSetInformationFetcher.GetGearSetInformation(new Guid("30122448-70c8-421c-bd8c-820e2905858b"));

            Assert.AreEqual(gsi.SlotGear["Body"].name, "Asphodelos Chiton of Healing");
        }
    }
}
