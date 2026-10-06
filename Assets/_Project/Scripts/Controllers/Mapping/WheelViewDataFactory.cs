using System;
using System.Collections.Generic;
using UnityEngine;
using Vertigo.Wheel.Core.Wheels;
using Vertigo.Wheel.Data.Rewards;
using Vertigo.Wheel.Data.Wheels;
using Vertigo.Wheel.Presentation.Common;
using Vertigo.Wheel.Presentation.Wheel;

namespace Vertigo.Wheel.Controllers.Mapping
{
    /// <summary>
    /// Combines the resolved wheel (amounts already scaled for the zone) with the authored visuals.
    /// </summary>
    public sealed class WheelViewDataFactory : IWheelViewDataProvider
    {
        private readonly ZoneProgressionSO _progression;
        private readonly IRewardCatalog _catalog;

        public WheelViewDataFactory(ZoneProgressionSO progression, IRewardCatalog catalog)
        {
            _progression = progression != null ? progression : throw new ArgumentNullException(nameof(progression));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public WheelViewData Create(int zone, WheelDefinition wheel)
        {
            WheelConfigSO config = _progression.GetWheelConfig(zone, wheel.ZoneType);
            var slices = new List<SliceViewData>(wheel.Slices.Count);

            foreach (WheelSlice slice in wheel.Slices)
            {
                slices.Add(slice.IsBomb
                    ? new SliceViewData(config.BombIcon, string.Empty, true)
                    : new SliceViewData(GetIcon(slice.RewardId), AmountFormatter.Multiplier(slice.Amount), false));
            }

            return new WheelViewData(config.BaseSprite, config.IndicatorSprite, config.Title, config.Subtitle, slices);
        }

        private Sprite GetIcon(string rewardId)
        {
            return _catalog.TryGet(rewardId, out RewardItemSO reward) ? reward.Icon : null;
        }
    }
}
