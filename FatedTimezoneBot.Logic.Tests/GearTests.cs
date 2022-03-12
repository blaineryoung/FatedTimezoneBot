using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Serializers;
using FatedTimezoneBot.Logic.Information.FileFetchers;

namespace FatedTimezoneBot.Logic.Tests
{
    public class GearTests
    {
        [Test]
        public async Task GetGearTest()
        {
            IGearInformationFetcher gf = new GearFileInformationFetcher();

            GearItem gi = await gf.GetGearInformation(35271);

            Assert.AreEqual(gi.name, "Asphodelos Himation of Maiming");
        }

        [Test]
        public async Task GetGearSetTest()
        {
            IGearInformationFetcher gf = new GearFileInformationFetcher();
            IGearSetInformationFetcher gearSetInformationFetcher = new GearSetFileInformationFetcher();
            IGearSlotMapperFactory gearSlotMapper = new GearSlotMapperFactory(gf);

            GearSetInfo gsi = await gearSetInformationFetcher.GetGearSetInformation(new Guid("30122448-70c8-421c-bd8c-820e2905858b"));

            GearSlotMap gsm = await gearSlotMapper.CreateGearSlotMap(gsi);

            Assert.AreEqual(gsm["Body"].name, "Asphodelos Chiton of Healing");
        }

        [Test]
        public async Task GetCharacterGearTest()
        {
            ICharacterInformationFetcher characterFetcher = new CharacterFileInformationFetcher();
            IGearInformationFetcher gf = new GearFileInformationFetcher();
            IGearSlotMapperFactory gearSlotMapper = new GearSlotMapperFactory(gf);

            CharacterInfo ci = await characterFetcher.GetCharacterInformation(19442264);

            GearSlotMap gsm = await gearSlotMapper.CreateGearSlotMap(ci);

            Assert.AreEqual(gsm["Body"].name, "Limbo Chiton of Healing");
        }

        [Test]
        public async Task GearDiffTEst()
        {
            ICharacterInformationFetcher characterFetcher = new CharacterFileInformationFetcher();
            IGearInformationFetcher gf = new GearFileInformationFetcher();
            IGearSlotMapperFactory gearSlotMapper = new GearSlotMapperFactory(gf);
            IGearSetInformationFetcher gearSetInformationFetcher = new GearSetFileInformationFetcher();

            CharacterInfo ci = await characterFetcher.GetCharacterInformation(19442264);
            GearSlotMap characterGear = await gearSlotMapper.CreateGearSlotMap(ci);

            GearSetInfo gsi = await gearSetInformationFetcher.GetGearSetInformation(new Guid("30122448-70c8-421c-bd8c-820e2905858b"));
            GearSlotMap gearSlotGear = await gearSlotMapper.CreateGearSlotMap(gsi);

            GearSlotMap diffSet = GearSlotMap.GenerateDiff(characterGear, gearSlotGear);

            Assert.AreEqual(7, diffSet.Count);
        }
    }
}
