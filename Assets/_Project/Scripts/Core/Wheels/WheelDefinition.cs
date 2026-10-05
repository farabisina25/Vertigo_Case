using System;
using System.Collections.Generic;
using System.Linq;
using Vertigo.Wheel.Core.Zones;

namespace Vertigo.Wheel.Core.Wheels
{
    public sealed class WheelDefinition
    {
        private readonly WheelSlice[] _slices;

        public ZoneType ZoneType { get; }
        public IReadOnlyList<WheelSlice> Slices => _slices;
        public bool HasBomb { get; }

        public WheelDefinition(ZoneType zoneType, IEnumerable<WheelSlice> slices)
        {
            if (slices == null)
            {
                throw new ArgumentNullException(nameof(slices));
            }

            _slices = slices.ToArray();

            if (_slices.Length == 0)
            {
                throw new ArgumentException("A wheel needs at least one slice.", nameof(slices));
            }

            ZoneType = zoneType;
            HasBomb = _slices.Any(slice => slice.IsBomb);
        }
    }
}
