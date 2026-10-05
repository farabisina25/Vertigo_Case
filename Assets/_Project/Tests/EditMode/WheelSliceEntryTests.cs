using NUnit.Framework;
using UnityEngine;
using Vertigo.Wheel.Data.Rewards;
using Vertigo.Wheel.Data.Wheels;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelSliceEntryTests
    {
        private RewardItemSO _reward;

        [SetUp]
        public void SetUp()
        {
            _reward = ScriptableObject.CreateInstance<RewardItemSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_reward);
        }

        [TestCase(1f, 100)]
        [TestCase(1.5f, 150)]
        [TestCase(0.001f, 1)]
        public void GetAmount_ScalingSlice_MultipliesAndNeverDropsBelowOne(float multiplier, int expected)
        {
            var entry = new WheelSliceEntry(_reward, 100, 1f, scalesWithZone: true);

            Assert.AreEqual(expected, entry.GetAmount(multiplier));
        }

        [Test]
        public void GetAmount_NonScalingSlice_KeepsBaseAmount()
        {
            var entry = new WheelSliceEntry(_reward, 1, 1f, scalesWithZone: false);

            Assert.AreEqual(1, entry.GetAmount(5f));
        }

        [Test]
        public void ToSlice_Bomb_CreatesBombSlice()
        {
            Assert.IsTrue(WheelSliceEntry.CreateBomb(1f).ToSlice(1f).IsBomb);
        }

        [Test]
        public void TryGetError_RewardSliceWithoutReward_ReportsError()
        {
            var entry = new WheelSliceEntry(null, 1, 1f, true);

            Assert.IsTrue(entry.TryGetError(out _));
        }
    }
}
