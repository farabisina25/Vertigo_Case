using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.Wheel.Presentation.Rewards
{
    public interface IRewardListView
    {
        void SetEntries(IReadOnlyList<RewardEntryData> entries);

        void Upsert(RewardEntryData entry, bool animate);

        bool TryGetIconPosition(string rewardId, out Vector3 worldPosition);

        void Clear();
    }
}
