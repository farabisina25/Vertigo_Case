namespace Vertigo.Wheel.Data.Rewards
{
    public interface IRewardCatalog
    {
        bool TryGet(string rewardId, out RewardItemSO reward);
    }
}
