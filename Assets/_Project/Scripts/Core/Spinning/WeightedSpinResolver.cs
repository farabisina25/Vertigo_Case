using System;
using System.Collections.Generic;
using Vertigo.Wheel.Core.Wheels;

namespace Vertigo.Wheel.Core.Spinning
{
    public sealed class WeightedSpinResolver : ISpinResolver
    {
        private readonly IRandomProvider _random;

        public WeightedSpinResolver(IRandomProvider random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public int ResolveSliceIndex(IReadOnlyList<WheelSlice> slices)
        {
            if (slices == null || slices.Count == 0)
            {
                throw new ArgumentException("Cannot resolve a spin without slices.", nameof(slices));
            }

            float totalWeight = 0f;
            for (int i = 0; i < slices.Count; i++)
            {
                totalWeight += slices[i].Weight;
            }

            if (totalWeight <= 0f)
            {
                throw new InvalidOperationException("At least one slice must have a positive weight.");
            }

            double roll = _random.NextDouble() * totalWeight;
            double cumulative = 0d;
            int lastSelectable = -1;

            for (int i = 0; i < slices.Count; i++)
            {
                if (slices[i].Weight <= 0f)
                {
                    continue;
                }

                cumulative += slices[i].Weight;
                lastSelectable = i;

                if (roll < cumulative)
                {
                    return i;
                }
            }

            // Floating point accumulation can leave roll marginally above the final cumulative value.
            return lastSelectable;
        }
    }
}
