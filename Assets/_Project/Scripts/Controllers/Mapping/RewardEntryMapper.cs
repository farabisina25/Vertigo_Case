using System;
using System.Collections.Generic;
using UnityEngine;
using Vertigo.Wheel.Core.Rewards;
using Vertigo.Wheel.Data.Rewards;
using Vertigo.Wheel.Presentation.Rewards;

namespace Vertigo.Wheel.Controllers.Mapping
{
    public sealed class RewardEntryMapper
    {
        private readonly IRewardCatalog _catalog;

        public RewardEntryMapper(IRewardCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public RewardEntryData Map(RewardStack stack)
        {
            return new RewardEntryData(stack.RewardId, GetIcon(stack.RewardId), stack.Amount);
        }

        public IReadOnlyList<RewardEntryData> Map(IReadOnlyList<RewardStack> stacks)
        {
            var entries = new List<RewardEntryData>(stacks.Count);
            foreach (RewardStack stack in stacks)
            {
                entries.Add(Map(stack));
            }

            return entries;
        }

        public Sprite GetIcon(string rewardId)
        {
            return _catalog.TryGet(rewardId, out RewardItemSO reward) ? reward.Icon : null;
        }
    }
}
