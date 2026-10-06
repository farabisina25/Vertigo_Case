using Vertigo.Wheel.Core.Zones;

namespace Vertigo.Wheel.Presentation.Zones
{
    public interface IZoneBarView
    {
        void Render(int currentZone, IZoneRules zoneRules);
    }
}
