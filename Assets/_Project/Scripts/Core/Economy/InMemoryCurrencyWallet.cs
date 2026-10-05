using System;

namespace Vertigo.Wheel.Core.Economy
{
    public class InMemoryCurrencyWallet : ICurrencyWallet
    {
        public event Action<int> BalanceChanged;

        public int Balance { get; private set; }

        public InMemoryCurrencyWallet(int initialBalance = 0)
        {
            if (initialBalance < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialBalance), "Balance cannot be negative.");
            }

            Balance = initialBalance;
        }

        public void Add(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
            }

            if (amount == 0)
            {
                return;
            }

            SetBalance(Balance + amount);
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
            }

            if (amount > Balance)
            {
                return false;
            }

            if (amount > 0)
            {
                SetBalance(Balance - amount);
            }

            return true;
        }

        protected virtual void SetBalance(int balance)
        {
            Balance = balance;
            BalanceChanged?.Invoke(Balance);
        }
    }
}
