using System;
using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Core.Zones;

namespace Vertigo.Wheel.Presentation.Zones
{
    /// <summary>
    /// Recycles a fixed set of cells around the current zone and slides them when the zone advances.
    /// </summary>
    public sealed class ZoneBarView : MonoBehaviour
    {
        [SerializeField] private RectTransform _scrollContent;
        [SerializeField] private ZoneCellView[] _cells = Array.Empty<ZoneCellView>();
        [Tooltip("Index of the cell that shows the current zone.")]
        [Min(0)] [SerializeField] private int _currentCellIndex = 3;
        [SerializeField] private ZoneTypeStyle[] _styles = Array.Empty<ZoneTypeStyle>();
        [Min(0f)] [SerializeField] private float _slideDuration = 0.35f;
        [SerializeField] private Ease _slideEase = Ease.OutCubic;

        private float _restX;
        private int _renderedZone;
        private Tween _slideTween;

        private void Awake()
        {
            _restX = _scrollContent.anchoredPosition.x;
        }

        public void Render(int currentZone, IZoneRules zoneRules)
        {
            bool advancedByOne = _renderedZone > 0 && currentZone == _renderedZone + 1;
            _renderedZone = currentZone;

            for (int i = 0; i < _cells.Length; i++)
            {
                int zone = currentZone - _currentCellIndex + i;
                if (zone < ZoneRules.FirstZone)
                {
                    _cells[i].RenderEmpty();
                    continue;
                }

                _cells[i].Render(zone, GetStyle(zoneRules.GetZoneType(zone)), zone == currentZone);
            }

            if (advancedByOne)
            {
                Slide();
            }
            else
            {
                _slideTween?.Kill();
                SetContentX(_restX);
            }
        }

        private void Slide()
        {
            _slideTween?.Kill();
            SetContentX(_restX + GetCellStep());
            _slideTween = _scrollContent.DOAnchorPosX(_restX, _slideDuration)
                .SetEase(_slideEase)
                .SetLink(gameObject);
        }

        private float GetCellStep()
        {
            if (_cells.Length < 2)
            {
                return 0f;
            }

            var first = (RectTransform)_cells[0].transform;
            var second = (RectTransform)_cells[1].transform;
            return second.anchoredPosition.x - first.anchoredPosition.x;
        }

        private void SetContentX(float x)
        {
            Vector2 position = _scrollContent.anchoredPosition;
            position.x = x;
            _scrollContent.anchoredPosition = position;
        }

        private ZoneTypeStyle GetStyle(ZoneType zoneType)
        {
            foreach (ZoneTypeStyle style in _styles)
            {
                if (style.ZoneType == zoneType)
                {
                    return style;
                }
            }

            Debug.LogWarning($"{name}: no style for {zoneType}.", this);
            return default;
        }

        private void OnValidate()
        {
            if (_cells.Length > 0)
            {
                _currentCellIndex = Mathf.Clamp(_currentCellIndex, 0, _cells.Length - 1);
            }
        }
    }
}
