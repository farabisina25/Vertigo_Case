using System;

namespace Vertigo.Wheel.Presentation.Wheel
{
    /// <summary>
    /// Slice i rests at -i * sliceAngle (clockwise from the top), so a wheel rotation of
    /// i * sliceAngle brings it under the indicator.
    /// </summary>
    public static class WheelAngles
    {
        public const float FullTurn = 360f;

        public static float GetSliceAngle(int sliceCount)
        {
            if (sliceCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sliceCount));
            }

            return FullTurn / sliceCount;
        }

        public static float GetSliceRestAngle(int sliceIndex, int sliceCount)
        {
            return -sliceIndex * GetSliceAngle(sliceCount);
        }

        /// <summary>
        /// Returns a z rotation below <paramref name="currentAngle"/> (clockwise spin) that stops on the slice.
        /// </summary>
        public static float GetClockwiseTarget(float currentAngle, int sliceIndex, int sliceCount, int fullRotations, float offset)
        {
            float landingAngle = sliceIndex * GetSliceAngle(sliceCount);
            float clockwiseDistance = Repeat(currentAngle - landingAngle, FullTurn);
            return currentAngle - clockwiseDistance - fullRotations * FullTurn + offset;
        }

        private static float Repeat(float value, float length)
        {
            float result = value % length;
            return result < 0f ? result + length : result;
        }
    }
}
