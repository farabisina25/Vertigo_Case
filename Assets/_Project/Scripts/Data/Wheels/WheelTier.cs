using System;
using UnityEngine;

namespace Vertigo.Wheel.Data.Wheels
{
    /// <summary>
    /// Wheels used from <see cref="FromZone"/> until the next tier starts.
    /// </summary>
    [Serializable]
    public sealed class WheelTier
    {
        [Min(1)] [SerializeField] private int _fromZone = 1;
        [SerializeField] private WheelConfigSO _normalWheel;
        [SerializeField] private WheelConfigSO _safeWheel;

        public int FromZone => _fromZone;
        public WheelConfigSO NormalWheel => _normalWheel;
        public WheelConfigSO SafeWheel => _safeWheel;
    }
}
