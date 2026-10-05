using System.Collections.Generic;
using Vertigo.Wheel.Core.Rewards;

namespace Vertigo.Wheel.Tests.Fakes
{
    internal sealed class SpyClaimHandler : IRewardClaimHandler
    {
        public IReadOnlyList<RewardStack> LastClaim { get; private set; }
        public int ClaimCount { get; private set; }

        public void Claim(IReadOnlyList<RewardStack> rewards)
        {
            LastClaim = rewards;
            ClaimCount++;
        }
    }
}
