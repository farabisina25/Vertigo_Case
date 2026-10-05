using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vertigo.Wheel.Core.Wheels;

namespace Vertigo.Wheel.Data.Wheels
{
    [CreateAssetMenu(fileName = "wheel_", menuName = "Vertigo/Wheel/Wheel Config")]
    public sealed class WheelConfigSO : ScriptableObject
    {
        public const int SliceCount = 8;

        [Header("Visuals")]
        [SerializeField] private Sprite _baseSprite;
        [SerializeField] private Sprite _indicatorSprite;
        [SerializeField] private Sprite _bombIcon;
        [SerializeField] private string _title;
        [SerializeField] private string _subtitle;

        [Header("Content")]
        [SerializeField] private List<WheelSliceEntry> _slices = new List<WheelSliceEntry>();

        public Sprite BaseSprite => _baseSprite;
        public Sprite IndicatorSprite => _indicatorSprite;
        public Sprite BombIcon => _bombIcon;
        public string Title => _title;
        public string Subtitle => _subtitle;
        public IReadOnlyList<WheelSliceEntry> Slices => _slices;
        public int BombCount => _slices.Count(slice => slice != null && slice.IsBomb);

        public IEnumerable<WheelSlice> CreateSlices(float zoneMultiplier)
        {
            return _slices.Select(slice => slice.ToSlice(zoneMultiplier));
        }

        private void OnValidate()
        {
            if (_slices.Count != SliceCount)
            {
                Debug.LogWarning($"{name} has {_slices.Count} slices, the wheel art is drawn for {SliceCount}.", this);
            }

            for (int i = 0; i < _slices.Count; i++)
            {
                if (_slices[i] != null && _slices[i].TryGetError(out string error))
                {
                    Debug.LogWarning($"{name} slice {i}: {error}", this);
                }
            }
        }
    }
}
