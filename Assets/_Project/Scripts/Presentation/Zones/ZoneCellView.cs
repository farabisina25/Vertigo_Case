using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Presentation.Zones
{
    public sealed class ZoneCellView : MonoBehaviour
    {
        [SerializeField] private Image _backgroundValue;
        [SerializeField] private Image _currentFrameValue;
        [SerializeField] private TMP_Text _zoneValue;

        public void Render(int zone, ZoneTypeStyle style, bool isCurrent)
        {
            _backgroundValue.enabled = true;
            _backgroundValue.sprite = style.Background;
            _currentFrameValue.enabled = isCurrent;
            _zoneValue.text = zone.ToString(CultureInfo.InvariantCulture);
            _zoneValue.color = style.TextColor;
        }

        public void RenderEmpty()
        {
            _backgroundValue.enabled = false;
            _currentFrameValue.enabled = false;
            _zoneValue.text = string.Empty;
        }
    }
}
