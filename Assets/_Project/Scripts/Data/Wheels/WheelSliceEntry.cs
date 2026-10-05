using System;
using UnityEngine;
using Vertigo.Wheel.Core.Wheels;
using Vertigo.Wheel.Data.Rewards;

namespace Vertigo.Wheel.Data.Wheels
{
    [Serializable]
    public sealed class WheelSliceEntry
    {
        [SerializeField] private bool _isBomb;
        [SerializeField] private RewardItemSO _reward;
        [Min(1)] [SerializeField] private int _baseAmount = 1;
        [Min(0f)] [SerializeField] private float _weight = 1f;
        [Tooltip("Multiply the amount by the zone reward multiplier.")]
        [SerializeField] private bool _scalesWithZone = true;

        public bool IsBomb => _isBomb;
        public RewardItemSO Reward => _reward;
        public int BaseAmount => _baseAmount;
        public float Weight => _weight;
        public bool ScalesWithZone => _scalesWithZone;

        public WheelSliceEntry()
        {
        }

        public WheelSliceEntry(RewardItemSO reward, int baseAmount, float weight, bool scalesWithZone)
        {
            _reward = reward;
            _baseAmount = baseAmount;
            _weight = weight;
            _scalesWithZone = scalesWithZone;
        }

        public static WheelSliceEntry CreateBomb(float weight)
        {
            return new WheelSliceEntry { _isBomb = true, _weight = weight };
        }

        public int GetAmount(float zoneMultiplier)
        {
            if (!_scalesWithZone)
            {
                return _baseAmount;
            }

            return Mathf.Max(1, Mathf.RoundToInt(_baseAmount * zoneMultiplier));
        }

        public WheelSlice ToSlice(float zoneMultiplier)
        {
            return _isBomb
                ? WheelSlice.Bomb(_weight)
                : WheelSlice.Reward(_reward.Id, GetAmount(zoneMultiplier), _weight);
        }

        public bool TryGetError(out string error)
        {
            if (!_isBomb && _reward == null)
            {
                error = "Reward slice has no reward assigned.";
                return true;
            }

            error = null;
            return false;
        }
    }
}
