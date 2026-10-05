using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Vertigo.Wheel.Core.Zones;
using Vertigo.Wheel.Data.Wheels;

namespace Vertigo.Wheel.Tests
{
    /// <summary>
    /// Guards the authored wheel assets so designers cannot ship an invalid configuration.
    /// </summary>
    public sealed class WheelContentValidationTests
    {
        private static IEnumerable<ZoneProgressionSO> Progressions()
        {
            return AssetDatabase.FindAssets("t:" + nameof(ZoneProgressionSO))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ZoneProgressionSO>);
        }

        private static IEnumerable<WheelConfigSO> Wheels()
        {
            return AssetDatabase.FindAssets("t:" + nameof(WheelConfigSO))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<WheelConfigSO>);
        }

        [Test]
        public void AtLeastOneProgressionExists()
        {
            Assert.IsTrue(Progressions().Any());
        }

        [Test]
        public void EveryWheel_HasExpectedSliceCount_AndValidSlices()
        {
            foreach (WheelConfigSO wheel in Wheels())
            {
                Assert.AreEqual(WheelConfigSO.SliceCount, wheel.Slices.Count, wheel.name);

                foreach (WheelSliceEntry slice in wheel.Slices)
                {
                    Assert.IsFalse(slice.TryGetError(out string error), $"{wheel.name}: {error}");
                }
            }
        }

        [Test]
        public void EveryProgression_HasOneBombOnNormalWheels_AndNoneOnRiskFreeWheels()
        {
            foreach (ZoneProgressionSO progression in Progressions())
            {
                Assert.IsNotEmpty(progression.Tiers, progression.name);
                Assert.AreEqual(ZoneRules.FirstZone, progression.Tiers[0].FromZone, progression.name);

                foreach (WheelTier tier in progression.Tiers)
                {
                    Assert.AreEqual(1, tier.NormalWheel.BombCount, tier.NormalWheel.name);
                    Assert.AreEqual(0, tier.SafeWheel.BombCount, tier.SafeWheel.name);
                }

                Assert.AreEqual(0, progression.SuperWheel.BombCount, progression.SuperWheel.name);
            }
        }

        [Test]
        public void Provider_BuildsAValidWheelForTheFirstHundredZones()
        {
            foreach (ZoneProgressionSO progression in Progressions())
            {
                IZoneRules rules = progression.CreateZoneRules();
                var provider = new ScriptableWheelProvider(progression);

                for (int zone = ZoneRules.FirstZone; zone <= 100; zone++)
                {
                    ZoneType type = rules.GetZoneType(zone);
                    var wheel = provider.GetWheel(zone, type);

                    Assert.AreEqual(type != ZoneType.Normal, !wheel.HasBomb, $"{progression.name} zone {zone}");
                }
            }
        }
    }
}
