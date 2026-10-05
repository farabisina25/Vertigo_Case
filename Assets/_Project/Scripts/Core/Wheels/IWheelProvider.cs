using Vertigo.Wheel.Core.Zones;

namespace Vertigo.Wheel.Core.Wheels
{
    public interface IWheelProvider
    {
        WheelDefinition GetWheel(int zone, ZoneType zoneType);
    }
}
