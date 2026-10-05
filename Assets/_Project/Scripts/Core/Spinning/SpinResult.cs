using Vertigo.Wheel.Core.Wheels;

namespace Vertigo.Wheel.Core.Spinning
{
    public readonly struct SpinResult
    {
        public int SliceIndex { get; }
        public WheelSlice Slice { get; }

        public SpinResult(int sliceIndex, WheelSlice slice)
        {
            SliceIndex = sliceIndex;
            Slice = slice;
        }
    }
}
