using System;
using NUnit.Framework;
using Vertigo.Wheel.Core.Rewards;

namespace Vertigo.Wheel.Tests
{
    public sealed class RewardInventoryTests
    {
        private RewardInventory _inventory;

        [SetUp]
        public void SetUp()
        {
            _inventory = new RewardInventory();
        }

        [Test]
        public void Add_SameRewardTwice_StacksAmounts()
        {
            _inventory.Add("gold", 10);
            RewardStack stack = _inventory.Add("gold", 5);

            Assert.AreEqual(15, stack.Amount);
            Assert.AreEqual(15, _inventory.GetAmount("gold"));
            Assert.AreEqual(1, _inventory.Stacks.Count);
        }

        [Test]
        public void Add_DifferentRewards_PreservesFirstCollectedOrder()
        {
            _inventory.Add("gold", 1);
            _inventory.Add("cash", 1);
            _inventory.Add("gold", 1);

            Assert.AreEqual("gold", _inventory.Stacks[0].RewardId);
            Assert.AreEqual("cash", _inventory.Stacks[1].RewardId);
        }

        [Test]
        public void Clear_RemovesEverything()
        {
            _inventory.Add("gold", 10);

            _inventory.Clear();

            Assert.IsTrue(_inventory.IsEmpty);
            Assert.AreEqual(0, _inventory.GetAmount("gold"));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Add_NonPositiveAmount_Throws(int amount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _inventory.Add("gold", amount));
        }
    }
}
