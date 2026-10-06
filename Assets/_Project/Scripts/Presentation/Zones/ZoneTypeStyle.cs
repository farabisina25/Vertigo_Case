using System;
using UnityEngine;
using Vertigo.Wheel.Core.Zones;

namespace Vertigo.Wheel.Presentation.Zones
{
    [Serializable]
    public struct ZoneTypeStyle
    {
        [SerializeField] private ZoneType _zoneType;
        [SerializeField] private Sprite _background;
        [SerializeField] private Color _textColor;

        public ZoneType ZoneType => _zoneType;
        public Sprite Background => _background;
        public Color TextColor => _textColor;
    }
}
