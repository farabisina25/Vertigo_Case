using System;
using Vertigo.Wheel.Core.Wheels;
using Vertigo.Wheel.Core.Zones;

namespace Vertigo.Wheel.Data.Wheels
{
    public sealed class ScriptableWheelProvider : IWheelProvider
    {
        private readonly ZoneProgressionSO _progression;

        public ScriptableWheelProvider(ZoneProgressionSO progression)
        {
            _progression = progression != null ? progression : throw new ArgumentNullException(nameof(progression));
        }

        public WheelDefinition GetWheel(int zone, ZoneType zoneType)
        {
            WheelConfigSO config = _progression.GetWheelConfig(zone, zoneType);
            float multiplier = _progression.GetRewardMultiplier(zone);

            return new WheelDefinition(zoneType, config.CreateSlices(multiplier));
        }
    }
}
