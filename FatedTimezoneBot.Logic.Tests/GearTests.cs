using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FatedTimezoneBot.Logic.Information;
using FatedTimezoneBot.Logic.Information.Serializers;
using FatedTimezoneBot.Logic.Information.FileFetchers;
using Microsoft.Extensions.Logging;

namespace FatedTimezoneBot.Logic.Tests
{
    public class GearTests
    {
        ILogger logger = DiscordTestUtilities.GetLogger();

        [Test]
        public async Task GetGearTest()
        {
            IGearInformationFetcher gf = new GearFileInformationFetcher(logger);

            GearItem gi = await gf.GetGearInformation(35271);

            Assert.AreEqual(gi.name, "Asphodelos Himation of Maiming");
        }

        [Test]
        public async Task MissingGearTest()
        {
            IGearInformationFetcher gf = new GearFileInformationFetcher(logger);

            GearItem gi = await gf.GetGearInformation(42);

            Assert.AreEqual(gi.name, "Unknown");
        }

        [Test]
        public async Task GetGearSetTest()
        {
            IGearInformationFetcher gf = new GearFileInformationFetcher(logger);
            IGearSetInformationFetcher gearSetInformationFetcher = new GearSetFileInformationFetcher();
            IGearSlotMapperFactory gearSlotMapper = new GearSlotMapperFactory(gf, logger);

            GearSetInfo gsi = await gearSetInformationFetcher.GetGearSetInformation(new Guid("30122448-70c8-421c-bd8c-820e2905858b"));

            GearSlotMap gsm = await gearSlotMapper.CreateGearSlotMap(gsi);
            GearItem body = gsm.Gear.Where(x => 0 == string.Compare(x.name, "Asphodelos Chiton of Healing", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            Assert.IsNotNull(body);
        }

        [Test]
        public async Task GetCharacterGearTest()
        {
            ICharacterInformationFetcher characterFetcher = new CharacterFileInformationFetcher();
            IGearInformationFetcher gf = new GearFileInformationFetcher(logger);
            IGearSlotMapperFactory gearSlotMapper = new GearSlotMapperFactory(gf, logger);

            CharacterInfo ci = await characterFetcher.GetCharacterInformation(19442264);

            GearSlotMap gsm = await gearSlotMapper.CreateGearSlotMap(ci);

            GearItem body = gsm.Gear.Where(x => 0 == string.Compare(x.name, "Limbo Chiton of Healing", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            Assert.IsNotNull(body);
        }

        [Test]
        public async Task NullingCharacterTest()
        {
            ICharacterInformationFetcher characterFetcher = new CharacterFileInformationFetcher();
            IGearInformationFetcher gf = new GearFileInformationFetcher(logger);
            IGearSlotMapperFactory gearSlotMapper = new GearSlotMapperFactory(gf, logger);

            CharacterInfo ci = await characterFetcher.GetCharacterInformation(30340081);

            Assert.IsNotNull(ci);
        }

        [Test]
        public async Task GearDiffTEst()
        {
            ICharacterInformationFetcher characterFetcher = new CharacterFileInformationFetcher();
            IGearInformationFetcher gf = new GearFileInformationFetcher(logger);
            IGearSlotMapperFactory gearSlotMapper = new GearSlotMapperFactory(gf, logger);
            IGearSetInformationFetcher gearSetInformationFetcher = new GearSetFileInformationFetcher();

            CharacterInfo ci = await characterFetcher.GetCharacterInformation(19442264);
            GearSlotMap characterGear = await gearSlotMapper.CreateGearSlotMap(ci);

            GearSetInfo gsi = await gearSetInformationFetcher.GetGearSetInformation(new Guid("30122448-70c8-421c-bd8c-820e2905858b"));
            GearSlotMap gearSlotGear = await gearSlotMapper.CreateGearSlotMap(gsi);

            GearSlotMap diffSet = GearSlotMap.GenerateDiff(characterGear, gearSlotGear);

            Assert.AreEqual(7, diffSet.Count);
        }

        [Test]
        public async Task GearDiffRingTest()
        {
            ICharacterInformationFetcher characterFetcher = new CharacterFileInformationFetcher();
            IGearInformationFetcher gf = new GearFileInformationFetcher(logger);
            IGearSlotMapperFactory gearSlotMapper = new GearSlotMapperFactory(gf, logger);
            IGearSetInformationFetcher gearSetInformationFetcher = new GearSetFileInformationFetcher();

            CharacterInfo ci = await characterFetcher.GetCharacterInformation(38987737);
            GearSlotMap characterGear = await gearSlotMapper.CreateGearSlotMap(ci);

            GearSetInfo gsi = await gearSetInformationFetcher.GetGearSetInformation(new Guid("d483c05e-a2ef-4fe0-906f-b883566586af"));
            GearSlotMap gearSlotGear = await gearSlotMapper.CreateGearSlotMap(gsi);

            GearSlotMap diffSet = GearSlotMap.GenerateDiff(characterGear, gearSlotGear);

            Assert.AreEqual(8, diffSet.Count);
        }

        [Test]
        public async Task UnknownGearTest()
        {
            ICharacterInformationFetcher characterFetcher = new CharacterFileInformationFetcher();
            IGearInformationFetcher gf = new GearFileInformationFetcher(logger);
            IGearSlotMapperFactory gearSlotMapper = new GearSlotMapperFactory(gf, logger);
            IGearSetInformationFetcher gearSetInformationFetcher = new GearSetFileInformationFetcher();

            CharacterInfo ci = await characterFetcher.GetCharacterInformation(42);
            GearSlotMap characterGear = await gearSlotMapper.CreateGearSlotMap(ci);

            GearSetInfo gsi = await gearSetInformationFetcher.GetGearSetInformation(new Guid("d483c05e-a2ef-4fe0-906f-b883566586af"));
            GearSlotMap gearSlotGear = await gearSlotMapper.CreateGearSlotMap(gsi);

            GearSlotMap diffSet = GearSlotMap.GenerateDiff(characterGear, gearSlotGear);

            Assert.AreEqual(6, diffSet.Count);
        }
    }
}
