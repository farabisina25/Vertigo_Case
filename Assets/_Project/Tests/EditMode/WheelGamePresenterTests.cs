using NUnit.Framework;
using UnityEngine;
using Vertigo.Wheel.Controllers.Game;
using Vertigo.Wheel.Controllers.Mapping;
using Vertigo.Wheel.Core.Economy;
using Vertigo.Wheel.Core.Game;
using Vertigo.Wheel.Core.Rewards;
using Vertigo.Wheel.Core.Zones;
using Vertigo.Wheel.Presentation.Common;
using Vertigo.Wheel.Tests.Fakes;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelGamePresenterTests
    {
        private const int ReviveCost = 100;

        private FixedSpinResolver _resolver;
        private InMemoryCurrencyWallet _wallet;
        private WheelGameModel _game;
        private GameTextsSO _texts;

        private FakeWheelView _wheel;
        private FakeZoneBarView _zoneBar;
        private FakeRewardListView _rewards;
        private FakeBombPopupView _bombPopup;
        private FakeSummaryPopupView _summary;
        private FakeLeaveButtonView _leave;
        private FakeWalletView _walletView;
        private WheelGamePresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            var zoneRules = new ZoneRules(5, 30);
            _resolver = new FixedSpinResolver { NextIndex = StubWheelProvider.GoldIndex };
            _wallet = new InMemoryCurrencyWallet();
            _game = new WheelGameModel(
                zoneRules,
                new StubWheelProvider(),
                _resolver,
                new RewardInventory(),
                new CurrencyReviveService(_wallet, ReviveCost, 0),
                new CurrencyClaimHandler(_wallet, StubWheelProvider.GoldId));

            _texts = ScriptableObject.CreateInstance<GameTextsSO>();
            _wheel = new FakeWheelView();
            _zoneBar = new FakeZoneBarView();
            _rewards = new FakeRewardListView();
            _bombPopup = new FakeBombPopupView();
            _summary = new FakeSummaryPopupView();
            _leave = new FakeLeaveButtonView();
            _walletView = new FakeWalletView();

            _presenter = new WheelGamePresenter(
                _game,
                zoneRules,
                _wallet,
                new FakeWheelViewDataProvider(),
                new RewardEntryMapper(new EmptyRewardCatalog()),
                _texts,
                new WheelGameViews(_wheel, _zoneBar, _rewards, _bombPopup, _summary, _leave, _walletView));

            _presenter.Initialize(StubWheelProvider.GoldId);
        }

        [TearDown]
        public void TearDown()
        {
            _presenter.Dispose();
            Object.DestroyImmediate(_texts);
        }

        private void SpinTo(int zone)
        {
            _resolver.NextIndex = StubWheelProvider.GoldIndex;
            while (_game.CurrentZone < zone)
            {
                _wheel.ClickSpin();
            }
        }

        private void HitBomb()
        {
            _resolver.NextIndex = StubWheelProvider.BombIndex;
            _wheel.ClickSpin();
        }

        [Test]
        public void Initialize_RendersFirstZoneWithSpinEnabledAndLeaveDisabled()
        {
            Assert.AreEqual(1, _zoneBar.LastZone);
            Assert.IsNotNull(_wheel.LastRender);
            Assert.IsTrue(_wheel.SpinInteractable);
            Assert.IsFalse(_leave.Available);
            Assert.IsFalse(_bombPopup.Visible);
            Assert.IsFalse(_summary.Visible);
        }

        [Test]
        public void SpinClick_SpinsWheelToResolvedSlice_AndBlocksInputUntilDone()
        {
            _wheel.CompleteSpinsImmediately = false;

            _wheel.ClickSpin();

            Assert.AreEqual(StubWheelProvider.GoldIndex, _wheel.LastSpinIndex);
            Assert.IsFalse(_wheel.SpinInteractable);
            Assert.IsFalse(_leave.Available);

            _wheel.PendingCompletion();

            Assert.IsTrue(_wheel.SpinInteractable);
            Assert.AreEqual(2, _zoneBar.LastZone);
            Assert.AreEqual(StubWheelProvider.GoldAmount, _rewards.Amounts[StubWheelProvider.GoldId]);
        }

        [Test]
        public void LeaveButton_IsOnlyAvailableOnRiskFreeZones()
        {
            SpinTo(4);
            Assert.IsFalse(_leave.Available);

            SpinTo(5);
            Assert.IsTrue(_leave.Available);

            SpinTo(6);
            Assert.IsFalse(_leave.Available);
        }

        [Test]
        public void Bomb_ShowsPopupWithReviveCost_AndReviveHidesIt()
        {
            _wallet.Add(ReviveCost);
            HitBomb();

            Assert.IsTrue(_bombPopup.Visible);
            Assert.AreEqual(ReviveCost, _bombPopup.ShownCost);
            Assert.IsTrue(_bombPopup.ShownCanRevive);
            Assert.IsFalse(_wheel.SpinInteractable);

            _bombPopup.ClickRevive();

            Assert.IsFalse(_bombPopup.Visible);
            Assert.IsTrue(_wheel.SpinInteractable);
            Assert.AreEqual(0, _walletView.Balance);
        }

        [Test]
        public void GiveUp_ShowsLostSummaryAndClearsRewards()
        {
            SpinTo(2);
            HitBomb();

            _bombPopup.ClickGiveUp();

            Assert.IsTrue(_summary.Visible);
            Assert.AreEqual(_texts.LostTitle, _summary.Title);
            Assert.IsEmpty(_rewards.Amounts);
        }

        [Test]
        public void Leave_ShowsCollectedSummary_AndCreditsCurrency()
        {
            SpinTo(5);

            _leave.Click();

            Assert.IsTrue(_summary.Visible);
            Assert.AreEqual(_texts.CollectedTitle, _summary.Title);
            Assert.AreEqual(1, _summary.Rewards.Count);
            Assert.AreEqual(StubWheelProvider.GoldAmount * 4, _walletView.Balance);
        }

        [Test]
        public void PlayAgain_ResetsToFirstZone()
        {
            SpinTo(5);
            _leave.Click();

            _summary.ClickPlayAgain();

            Assert.IsFalse(_summary.Visible);
            Assert.AreEqual(1, _zoneBar.LastZone);
            Assert.IsEmpty(_rewards.Amounts);
            Assert.AreEqual(GameState.Ready, _game.State);
        }
    }
}
