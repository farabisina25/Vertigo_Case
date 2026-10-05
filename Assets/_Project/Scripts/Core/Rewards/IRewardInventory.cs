using System.Collections.Generic;

namespace Vertigo.Wheel.Core.Rewards
{
    public interface IRewardInventory
    {
        IReadOnlyList<RewardStack> Stacks { get; }
        bool IsEmpty { get; }

        /// <returns>The stack after the amount has been added.</returns>
        RewardStack Add(string rewardId, int amount);

        int GetAmount(string rewardId);

        void Clear();
    }
}
