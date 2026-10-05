namespace Vertigo.Wheel.Core.Zones
{
    public interface IZoneRules
    {
        ZoneType GetZoneType(int zone);

        bool IsRiskFree(int zone);
    }
}
