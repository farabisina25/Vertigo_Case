using System;
using System.Collections.Generic;

namespace Vertigo.Wheel.Core.Rewards
{
    /// <summary>
    /// Stacks rewards by id while preserving the order in which they were first collected.
    /// </summary>
    public sealed class RewardInventory : IRewardInventory
    {
        private readonly List<RewardStack> _stacks = new List<RewardStack>();

        public IReadOnlyList<RewardStack> Stacks => _stacks;
        public bool IsEmpty => _stacks.Count == 0;

        public RewardStack Add(string rewardId, int amount)
        {
            if (string.IsNullOrEmpty(rewardId))
            {
                throw new ArgumentException("Reward id is required.", nameof(rewardId));
            }

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
            }

            int index = IndexOf(rewardId);
            if (index < 0)
            {
                var created = new RewardStack(rewardId, amount);
                _stacks.Add(created);
                return created;
            }

            var updated = new RewardStack(rewardId, _stacks[index].Amount + amount);
            _stacks[index] = updated;
            return updated;
        }

        public int GetAmount(string rewardId)
        {
            int index = IndexOf(rewardId);
            return index < 0 ? 0 : _stacks[index].Amount;
        }

        public void Clear()
        {
            _stacks.Clear();
        }

        private int IndexOf(string rewardId)
        {
            for (int i = 0; i < _stacks.Count; i++)
            {
                if (_stacks[i].RewardId == rewardId)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
