using NUnit.Framework;
using Vertigo.Wheel.Presentation.Common;
using Vertigo.Wheel.Presentation.Wheel;

namespace Vertigo.Wheel.Tests
{
    public sealed class PresentationMathTests
    {
        [TestCase(0, "0")]
        [TestCase(9999, "9999")]
        [TestCase(10000, "10K")]
        [TestCase(12345, "12.3K")]
        [TestCase(999999, "999.9K")]
        [TestCase(1250000, "1.2M")]
        public void Compact_FormatsAndFloors(int amount, string expected)
        {
            Assert.AreEqual(expected, AmountFormatter.Compact(amount));
        }

        [Test]
        public void Multiplier_PrefixesWithX()
        {
            Assert.AreEqual("x150", AmountFormatter.Multiplier(150));
        }

        [TestCase(0f, 0)]
        [TestCase(0f, 3)]
        [TestCase(123f, 5)]
        [TestCase(-725f, 7)]
        public void ClockwiseTarget_LandsTheSliceUnderTheIndicator(float current, int sliceIndex)
        {
            const int sliceCount = 8;
            float target = WheelAngles.GetClockwiseTarget(current, sliceIndex, sliceCount, fullRotations: 3, offset: 0f);

            float sliceWorldAngle = target + WheelAngles.GetSliceRestAngle(sliceIndex, sliceCount);
            float normalized = ((sliceWorldAngle % 360f) + 360f) % 360f;

            Assert.That(normalized < 0.01f || normalized > 359.99f, $"slice ended at {normalized} degrees");
        }

        [TestCase(0f, 0)]
        [TestCase(90f, 2)]
        public void ClockwiseTarget_AlwaysSpinsClockwiseForAtLeastTheFullRotations(float current, int sliceIndex)
        {
            float target = WheelAngles.GetClockwiseTarget(current, sliceIndex, 8, fullRotations: 4, offset: 0f);

            Assert.LessOrEqual(target, current - 4 * WheelAngles.FullTurn);
            Assert.Greater(target, current - 5 * WheelAngles.FullTurn);
        }
    }
}
