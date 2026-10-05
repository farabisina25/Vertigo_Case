using System;

namespace Vertigo.Wheel.Core.Zones
{
    /// <summary>
    /// Every Nth zone is safe, every Mth zone is super. Super takes precedence over safe.
    /// </summary>
    public sealed class ZoneRules : IZoneRules
    {
        public const int FirstZone = 1;

        private readonly int _safeZoneInterval;
        private readonly int _superZoneInterval;

        public ZoneRules(int safeZoneInterval, int superZoneInterval)
        {
            if (safeZoneInterval <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(safeZoneInterval), "Interval must be positive.");
            }

            if (superZoneInterval <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(superZoneInterval), "Interval must be positive.");
            }

            _safeZoneInterval = safeZoneInterval;
            _superZoneInterval = superZoneInterval;
        }

        public ZoneType GetZoneType(int zone)
        {
            if (zone < FirstZone)
            {
                throw new ArgumentOutOfRangeException(nameof(zone), $"Zones start at {FirstZone}.");
            }

            if (zone % _superZoneInterval == 0)
            {
                return ZoneType.Super;
            }

            return zone % _safeZoneInterval == 0 ? ZoneType.Safe : ZoneType.Normal;
        }

        public bool IsRiskFree(int zone)
        {
            return GetZoneType(zone) != ZoneType.Normal;
        }
    }
}
