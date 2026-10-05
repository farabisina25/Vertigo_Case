using NUnit.Framework;
using Vertigo.Wheel.Core.Economy;
using Vertigo.Wheel.Core.Rewards;

namespace Vertigo.Wheel.Tests
{
    public sealed class EconomyTests
    {
        [Test]
        public void Wallet_TrySpend_MoreThanBalance_FailsWithoutChangingBalance()
        {
            var wallet = new InMemoryCurrencyWallet(50);

            Assert.IsFalse(wallet.TrySpend(51));
            Assert.AreEqual(50, wallet.Balance);
        }

        [Test]
        public void Wallet_AddAndSpend_RaisesBalanceChanged()
        {
            var wallet = new InMemoryCurrencyWallet();
            int lastBalance = -1;
            wallet.BalanceChanged += balance => lastBalance = balance;

            wallet.Add(100);
            Assert.AreEqual(100, lastBalance);

            wallet.TrySpend(30);
            Assert.AreEqual(70, lastBalance);
        }

        [Test]
        public void Revive_CostIncreasesPerRevive_AndResets()
        {
            var wallet = new InMemoryCurrencyWallet(1000);
            var revive = new CurrencyReviveService(wallet, baseCost: 100, costIncreasePerRevive: 50);

            Assert.AreEqual(100, revive.CurrentCost);
            Assert.IsTrue(revive.TryRevive());
            Assert.AreEqual(150, revive.CurrentCost);
            Assert.AreEqual(900, wallet.Balance);

            revive.Reset();
            Assert.AreEqual(100, revive.CurrentCost);
        }

        [Test]
        public void Revive_NotEnoughCurrency_Fails()
        {
            var wallet = new InMemoryCurrencyWallet(99);
            var revive = new CurrencyReviveService(wallet, baseCost: 100, costIncreasePerRevive: 0);

            Assert.IsFalse(revive.CanAfford);
            Assert.IsFalse(revive.TryRevive());
            Assert.AreEqual(99, wallet.Balance);
        }

        [Test]
        public void ClaimHandler_CreditsOnlyMatchingCurrency()
        {
            var wallet = new InMemoryCurrencyWallet();
            var handler = new CurrencyClaimHandler(wallet, "gold");

            handler.Claim(new[]
            {
                new RewardStack("gold", 250),
                new RewardStack("cash", 40)
            });

            Assert.AreEqual(250, wallet.Balance);
        }
    }
}
