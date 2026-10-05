using System.Collections.Generic;
using UnityEngine;
using Vertigo.Wheel.Core.Zones;

namespace Vertigo.Wheel.Data.Wheels
{
    [CreateAssetMenu(fileName = "zone_progression", menuName = "Vertigo/Wheel/Zone Progression")]
    public sealed class ZoneProgressionSO : ScriptableObject
    {
        [Header("Zone Rules")]
        [Min(1)] [SerializeField] private int _safeZoneInterval = 5;
        [Min(1)] [SerializeField] private int _superZoneInterval = 30;

        [Header("Wheels")]
        [Tooltip("Ordered by FromZone. The first tier must start at zone 1.")]
        [SerializeField] private List<WheelTier> _tiers = new List<WheelTier>();
        [SerializeField] private WheelConfigSO _superWheel;

        [Header("Rewards")]
        [Tooltip("Reward amounts are multiplied by 1 + growth * (zone - 1).")]
        [Min(0f)] [SerializeField] private float _rewardGrowthPerZone = 0.1f;

        public int SafeZoneInterval => _safeZoneInterval;
        public int SuperZoneInterval => _superZoneInterval;
        public IReadOnlyList<WheelTier> Tiers => _tiers;
        public WheelConfigSO SuperWheel => _superWheel;

        public IZoneRules CreateZoneRules()
        {
            return new ZoneRules(_safeZoneInterval, _superZoneInterval);
        }

        public float GetRewardMultiplier(int zone)
        {
            return 1f + _rewardGrowthPerZone * (zone - ZoneRules.FirstZone);
        }

        public WheelConfigSO GetWheelConfig(int zone, ZoneType zoneType)
        {
            if (zoneType == ZoneType.Super)
            {
                return _superWheel;
            }

            WheelTier tier = GetTier(zone);
            return zoneType == ZoneType.Safe ? tier.SafeWheel : tier.NormalWheel;
        }

        private WheelTier GetTier(int zone)
        {
            WheelTier selected = _tiers[0];

            foreach (WheelTier tier in _tiers)
            {
                if (tier.FromZone > zone)
                {
                    break;
                }

                selected = tier;
            }

            return selected;
        }

        private void OnValidate()
        {
            if (_tiers.Count == 0 || _tiers[0].FromZone != ZoneRules.FirstZone)
            {
                Debug.LogError($"{name}: the first wheel tier must start at zone {ZoneRules.FirstZone}.", this);
            }

            for (int i = 0; i < _tiers.Count; i++)
            {
                WheelTier tier = _tiers[i];

                if (i > 0 && tier.FromZone <= _tiers[i - 1].FromZone)
                {
                    Debug.LogError($"{name}: tier {i} must start after tier {i - 1}.", this);
                }

                ValidateWheel(tier.NormalWheel, expectedBombs: 1, $"tier {i} normal wheel");
                ValidateWheel(tier.SafeWheel, expectedBombs: 0, $"tier {i} safe wheel");
            }

            ValidateWheel(_superWheel, expectedBombs: 0, "super wheel");
        }

        private void ValidateWheel(WheelConfigSO wheel, int expectedBombs, string label)
        {
            if (wheel == null)
            {
                Debug.LogError($"{name}: {label} is not assigned.", this);
                return;
            }

            if (wheel.BombCount != expectedBombs)
            {
                Debug.LogError($"{name}: {label} '{wheel.name}' must contain {expectedBombs} bomb(s).", this);
            }
        }
    }
}
