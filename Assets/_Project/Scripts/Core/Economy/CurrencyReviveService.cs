using System;

namespace Vertigo.Wheel.Core.Economy
{
    /// <summary>
    /// Revive cost grows linearly with each revive used in the same run.
    /// </summary>
    public sealed class CurrencyReviveService : IReviveService
    {
        private readonly ICurrencyWallet _wallet;
        private readonly int _baseCost;
        private readonly int _costIncreasePerRevive;
        private int _revivesUsed;

        public int CurrentCost => _baseCost + _costIncreasePerRevive * _revivesUsed;
        public bool CanAfford => _wallet.Balance >= CurrentCost;

        public CurrencyReviveService(ICurrencyWallet wallet, int baseCost, int costIncreasePerRevive)
        {
            if (baseCost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseCost), "Cost cannot be negative.");
            }

            if (costIncreasePerRevive < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(costIncreasePerRevive), "Increase cannot be negative.");
            }

            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _baseCost = baseCost;
            _costIncreasePerRevive = costIncreasePerRevive;
        }

        public bool TryRevive()
        {
            if (!_wallet.TrySpend(CurrentCost))
            {
                return false;
            }

            _revivesUsed++;
            return true;
        }

        public void Reset()
        {
            _revivesUsed = 0;
        }
    }
}
