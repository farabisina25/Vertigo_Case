using System;

namespace Vertigo.Wheel.Core.Economy
{
    public interface ICurrencyWallet
    {
        event Action<int> BalanceChanged;

        int Balance { get; }

        void Add(int amount);

        bool TrySpend(int amount);
    }
}
