using System.Collections.Generic;

namespace Vertigo.Wheel.Presentation.Rewards
{
    public interface IRewardListView
    {
        void SetEntries(IReadOnlyList<RewardEntryData> entries);

        void Upsert(RewardEntryData entry, bool animate);

        void Clear();
    }
}
