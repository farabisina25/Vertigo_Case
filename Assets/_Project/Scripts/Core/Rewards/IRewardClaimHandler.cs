using System.Collections.Generic;

namespace Vertigo.Wheel.Core.Rewards
{
    /// <summary>
    /// Receives the rewards the player walks away with (e.g. credits currencies to a wallet).
    /// </summary>
    public interface IRewardClaimHandler
    {
        void Claim(IReadOnlyList<RewardStack> rewards);
    }
}
