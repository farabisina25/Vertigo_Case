using System.Collections.Generic;
using Vertigo.Wheel.Core.Wheels;

namespace Vertigo.Wheel.Core.Spinning
{
    public interface ISpinResolver
    {
        int ResolveSliceIndex(IReadOnlyList<WheelSlice> slices);
    }
}
