namespace Vertigo.Wheel.Core.Rewards
{
    public readonly struct RewardStack
    {
        public string RewardId { get; }
        public int Amount { get; }

        public RewardStack(string rewardId, int amount)
        {
            RewardId = rewardId;
            Amount = amount;
        }
    }
}
