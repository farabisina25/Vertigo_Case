using System;
using System.Collections.Generic;
using NUnit.Framework;
using Vertigo.Wheel.Core.Economy;
using Vertigo.Wheel.Core.Game;
using Vertigo.Wheel.Core.Rewards;
using Vertigo.Wheel.Core.Zones;
using Vertigo.Wheel.Tests.Fakes;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelGameModelTests
    {
        private const int ReviveCost = 100;

        private StubWheelProvider _wheelProvider;
        private FixedSpinResolver _resolver;
        private InMemoryCurrencyWallet _wallet;
        private SpyClaimHandler _claimHandler;
        private WheelGameModel _game;

        [SetUp]
        public void SetUp()
        {
            _wheelProvider = new StubWheelProvider();
            _resolver = new FixedSpinResolver { NextIndex = StubWheelProvider.GoldIndex };
            _wallet = new InMemoryCurrencyWallet();
            _claimHandler = new SpyClaimHandler();
            _game = CreateGame();
        }

        private WheelGameModel CreateGame()
        {
            return new WheelGameModel(
                new ZoneRules(5, 30),
                _wheelProvider,
                _resolver,
                new RewardInventory(),
                new CurrencyReviveService(_wallet, ReviveCost, 0),
                _claimHandler);
        }

        private void SpinToCompletion()
        {
            Assert.IsTrue(_game.TryStartSpin());
            Assert.IsTrue(_game.TryCompleteSpin());
        }

        private void AdvanceToZone(int zone)
        {
            _resolver.NextIndex = StubWheelProvider.GoldIndex;
            while (_game.CurrentZone < zone)
            {
                SpinToCompletion();
            }
        }

        private void HitBomb()
        {
            _resolver.NextIndex = StubWheelProvider.BombIndex;
            SpinToCompletion();
        }

        [Test]
        public void NewGame_StartsReadyAtFirstZone()
        {
            Assert.AreEqual(GameState.Ready, _game.State);
            Assert.AreEqual(1, _game.CurrentZone);
            Assert.AreEqual(ZoneType.Normal, _game.CurrentZoneType);
            Assert.IsEmpty(_game.CollectedRewards);
        }

        [Test]
        public void TryStartSpin_RaisesSpinStartedWithResolvedSlice()
        {
            int startedIndex = -1;
            _game.SpinStarted += result => startedIndex = result.SliceIndex;

            _game.TryStartSpin();

            Assert.AreEqual(StubWheelProvider.GoldIndex, startedIndex);
            Assert.AreEqual(GameState.Spinning, _game.State);
        }

        [Test]
        public void WhileSpinning_CannotSpinOrLeave()
        {
            AdvanceToZone(5);
            _game.TryStartSpin();

            Assert.IsFalse(_game.TryStartSpin());
            Assert.IsFalse(_game.CanLeave);
            Assert.IsFalse(_game.TryLeave());
        }

        [Test]
        public void RewardIsAppliedOnlyAfterSpinCompletes()
        {
            _game.TryStartSpin();
            Assert.IsEmpty(_game.CollectedRewards);

            _game.TryCompleteSpin();

            Assert.AreEqual(StubWheelProvider.GoldAmount, _game.CollectedRewards[0].Amount);
            Assert.AreEqual(2, _game.CurrentZone);
            Assert.AreEqual(GameState.Ready, _game.State);
        }

        [Test]
        public void CompletingRewardSpin_RaisesRewardGainedAndZoneChanged()
        {
            RewardStack gained = default;
            int changedZone = 0;
            _game.RewardGained += stack => gained = stack;
            _game.ZoneChanged += zone => changedZone = zone;

            SpinToCompletion();

            Assert.AreEqual(StubWheelProvider.GoldId, gained.RewardId);
            Assert.AreEqual(2, changedZone);
        }

        [Test]
        public void CannotLeaveOnNormalZone()
        {
            AdvanceToZone(4);

            Assert.IsFalse(_game.CanLeave);
            Assert.IsFalse(_game.TryLeave());
        }

        [TestCase(5)]
        [TestCase(30)]
        public void CanLeaveOnRiskFreeZone_AndClaimsRewards(int zone)
        {
            AdvanceToZone(zone);

            Assert.IsTrue(_game.TryLeave());
            Assert.AreEqual(GameState.Collected, _game.State);
            Assert.AreEqual(1, _claimHandler.ClaimCount);
            Assert.AreEqual(StubWheelProvider.GoldAmount * (zone - 1), _claimHandler.LastClaim[0].Amount);
        }

        [Test]
        public void HittingBomb_EntersBombedStateWithoutAdvancingZone()
        {
            SpinToCompletion();

            HitBomb();

            Assert.AreEqual(GameState.Bombed, _game.State);
            Assert.AreEqual(2, _game.CurrentZone);
            Assert.IsFalse(_game.CanSpin);
        }

        [Test]
        public void GivingUpAfterBomb_LosesAllRewards()
        {
            SpinToCompletion();
            HitBomb();

            Assert.IsTrue(_game.TryGiveUp());

            Assert.AreEqual(GameState.Lost, _game.State);
            Assert.IsEmpty(_game.CollectedRewards);
        }

        [Test]
        public void ReviveWithEnoughCurrency_KeepsRewardsAndZone()
        {
            _wallet.Add(ReviveCost);
            SpinToCompletion();
            HitBomb();

            Assert.IsTrue(_game.CanRevive);
            Assert.IsTrue(_game.TryRevive());

            Assert.AreEqual(GameState.Ready, _game.State);
            Assert.AreEqual(2, _game.CurrentZone);
            Assert.IsNotEmpty(_game.CollectedRewards);
            Assert.AreEqual(0, _wallet.Balance);
        }

        [Test]
        public void ReviveWithoutEnoughCurrency_Fails()
        {
            HitBomb();

            Assert.IsFalse(_game.CanRevive);
            Assert.IsFalse(_game.TryRevive());
            Assert.AreEqual(GameState.Bombed, _game.State);
        }

        [Test]
        public void Restart_OnlyAfterRunEnded_ResetsToFirstZone()
        {
            Assert.IsFalse(_game.TryRestart());

            SpinToCompletion();
            HitBomb();
            _game.TryGiveUp();

            Assert.IsTrue(_game.TryRestart());
            Assert.AreEqual(1, _game.CurrentZone);
            Assert.AreEqual(GameState.Ready, _game.State);
        }

        [Test]
        public void RiskFreeZoneWithBomb_IsRejected()
        {
            _wheelProvider.PutBombOnRiskFreeWheels = true;

            Assert.Throws<InvalidOperationException>(() => AdvanceToZone(5));
        }

        [Test]
        public void StateChanged_FollowsSpinLifecycle()
        {
            var states = new List<GameState>();
            _game.StateChanged += states.Add;

            SpinToCompletion();

            CollectionAssert.AreEqual(new[] { GameState.Spinning, GameState.Ready }, states);
        }
    }
}
