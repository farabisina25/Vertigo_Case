using Vertigo.Wheel.Core.Wheels;
using Vertigo.Wheel.Core.Zones;

namespace Vertigo.Wheel.Tests.Fakes
{
    /// <summary>
    /// Normal wheels: slice 0 is the bomb, slice 1 is "gold" x10.
    /// Safe/super wheels: slice 0 is "chest" x1, slice 1 is "gold" x10.
    /// </summary>
    internal sealed class StubWheelProvider : IWheelProvider
    {
        public const int BombIndex = 0;
        public const int GoldIndex = 1;
        public const string GoldId = "gold";
        public const string ChestId = "chest";
        public const int GoldAmount = 10;

        public bool PutBombOnRiskFreeWheels { get; set; }

        public WheelDefinition GetWheel(int zone, ZoneType zoneType)
        {
            bool hasBomb = zoneType == ZoneType.Normal || PutBombOnRiskFreeWheels;
            WheelSlice first = hasBomb ? WheelSlice.Bomb(1f) : WheelSlice.Reward(ChestId, 1, 1f);

            return new WheelDefinition(zoneType, new[]
            {
                first,
                WheelSlice.Reward(GoldId, GoldAmount, 1f)
            });
        }
    }
}
