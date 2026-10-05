using System;
using System.Collections.Generic;
using Vertigo.Wheel.Core.Rewards;

namespace Vertigo.Wheel.Core.Economy
{
    /// <summary>
    /// Credits the claimed stacks of a single currency reward to the wallet.
    /// </summary>
    public sealed class CurrencyClaimHandler : IRewardClaimHandler
    {
        private readonly ICurrencyWallet _wallet;
        private readonly string _currencyRewardId;

        public CurrencyClaimHandler(ICurrencyWallet wallet, string currencyRewardId)
        {
            if (string.IsNullOrEmpty(currencyRewardId))
            {
                throw new ArgumentException("Currency reward id is required.", nameof(currencyRewardId));
            }

            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _currencyRewardId = currencyRewardId;
        }

        public void Claim(IReadOnlyList<RewardStack> rewards)
        {
            for (int i = 0; i < rewards.Count; i++)
            {
                if (rewards[i].RewardId == _currencyRewardId)
                {
                    _wallet.Add(rewards[i].Amount);
                }
            }
        }
    }
}
