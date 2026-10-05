using System;
using NUnit.Framework;
using Vertigo.Wheel.Core.Spinning;
using Vertigo.Wheel.Core.Wheels;
using Vertigo.Wheel.Tests.Fakes;

namespace Vertigo.Wheel.Tests
{
    public sealed class WeightedSpinResolverTests
    {
        private static readonly WheelSlice[] Slices =
        {
            WheelSlice.Reward("a", 1, 1f),
            WheelSlice.Reward("b", 1, 0f),
            WheelSlice.Reward("c", 1, 3f)
        };

        [TestCase(0.0, 0)]
        [TestCase(0.24, 0)]
        [TestCase(0.25, 2)]
        [TestCase(0.99, 2)]
        public void ResolveSliceIndex_PicksSliceByCumulativeWeight(double roll, int expectedIndex)
        {
            var resolver = new WeightedSpinResolver(new SequenceRandomProvider(roll));

            Assert.AreEqual(expectedIndex, resolver.ResolveSliceIndex(Slices));
        }

        [Test]
        public void ResolveSliceIndex_NeverPicksZeroWeightSlice()
        {
            var resolver = new WeightedSpinResolver(new SystemRandomProvider(seed: 42));

            for (int i = 0; i < 1000; i++)
            {
                Assert.AreNotEqual(1, resolver.ResolveSliceIndex(Slices));
            }
        }

        [Test]
        public void ResolveSliceIndex_AllWeightsZero_Throws()
        {
            var resolver = new WeightedSpinResolver(new SequenceRandomProvider(0.5));
            var slices = new[] { WheelSlice.Reward("a", 1, 0f), WheelSlice.Bomb(0f) };

            Assert.Throws<InvalidOperationException>(() => resolver.ResolveSliceIndex(slices));
        }

        [Test]
        public void ResolveSliceIndex_EmptySlices_Throws()
        {
            var resolver = new WeightedSpinResolver(new SequenceRandomProvider(0.5));

            Assert.Throws<ArgumentException>(() => resolver.ResolveSliceIndex(Array.Empty<WheelSlice>()));
        }
    }
}
