using UnityEngine;

namespace Vertigo.Wheel.Presentation.Rewards
{
    public readonly struct RewardEntryData
    {
        public string RewardId { get; }
        public Sprite Icon { get; }
        public int Amount { get; }

        public RewardEntryData(string rewardId, Sprite icon, int amount)
        {
            RewardId = rewardId;
            Icon = icon;
            Amount = amount;
        }
    }
}
