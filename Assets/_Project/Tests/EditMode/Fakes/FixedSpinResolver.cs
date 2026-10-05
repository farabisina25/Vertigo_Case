using System.Collections.Generic;
using Vertigo.Wheel.Core.Spinning;
using Vertigo.Wheel.Core.Wheels;

namespace Vertigo.Wheel.Tests.Fakes
{
    internal sealed class FixedSpinResolver : ISpinResolver
    {
        public int NextIndex { get; set; }

        public int ResolveSliceIndex(IReadOnlyList<WheelSlice> slices)
        {
            return NextIndex;
        }
    }
}
