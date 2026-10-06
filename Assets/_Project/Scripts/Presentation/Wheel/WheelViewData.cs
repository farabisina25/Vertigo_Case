using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.Wheel.Presentation.Wheel
{
    public sealed class WheelViewData
    {
        public Sprite BaseSprite { get; }
        public Sprite IndicatorSprite { get; }
        public string Title { get; }
        public string Subtitle { get; }
        public IReadOnlyList<SliceViewData> Slices { get; }

        public WheelViewData(
            Sprite baseSprite,
            Sprite indicatorSprite,
            string title,
            string subtitle,
            IReadOnlyList<SliceViewData> slices)
        {
            BaseSprite = baseSprite;
            IndicatorSprite = indicatorSprite;
            Title = title;
            Subtitle = subtitle;
            Slices = slices;
        }
    }
}
