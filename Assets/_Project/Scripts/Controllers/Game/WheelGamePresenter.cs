using System;
using UnityEngine;
using Vertigo.Wheel.Controllers.Mapping;
using Vertigo.Wheel.Core.Economy;
using Vertigo.Wheel.Core.Game;
using Vertigo.Wheel.Core.Rewards;
using Vertigo.Wheel.Core.Spinning;
using Vertigo.Wheel.Core.Zones;
using Vertigo.Wheel.Presentation.Common;
using Vertigo.Wheel.Presentation.Rewards;

namespace Vertigo.Wheel.Controllers.Game
{
    /// <summary>
    /// Translates model events into view updates and view input into model commands.
    /// Holds no game rules of its own.
    /// </summary>
    public sealed class WheelGamePresenter : IDisposable
    {
        private readonly IWheelGame _game;
        private readonly IZoneRules _zoneRules;
        private readonly ICurrencyWallet _wallet;
        private readonly IWheelViewDataProvider _wheelViewData;
        private readonly RewardEntryMapper _rewardMapper;
        private readonly GameTextsSO _texts;
        private readonly WheelGameViews _views;

        private GameState _lastState;
        private bool _initialized;
        private int _landedSliceIndex;
        private int _run;

        public WheelGamePresenter(
            IWheelGame game,
            IZoneRules zoneRules,
            ICurrencyWallet wallet,
            IWheelViewDataProvider wheelViewData,
            RewardEntryMapper rewardMapper,
            GameTextsSO texts,
            WheelGameViews views)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _zoneRules = zoneRules ?? throw new ArgumentNullException(nameof(zoneRules));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _wheelViewData = wheelViewData ?? throw new ArgumentNullException(nameof(wheelViewData));
            _rewardMapper = rewardMapper ?? throw new ArgumentNullException(nameof(rewardMapper));
            _texts = texts != null ? texts : throw new ArgumentNullException(nameof(texts));
            _views = views ?? throw new ArgumentNullException(nameof(views));
        }

        public void Initialize(string walletCurrencyId)
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            Subscribe();

            _views.BombPopup.HideImmediate();
            _views.SummaryPopup.HideImmediate();
            _views.Wallet.SetCurrencyIcon(_rewardMapper.GetIcon(walletCurrencyId));
            _views.Wallet.SetBalance(_wallet.Balance, animate: false);

            RenderZone();
            _views.CollectedRewards.SetEntries(_rewardMapper.Map(_game.CollectedRewards));
            HandleStateChanged(_game.State);
        }

        public void Dispose()
        {
            if (!_initialized)
            {
                return;
            }

            _initialized = false;
            Unsubscribe();
        }

        private void Subscribe()
        {
            _game.StateChanged += HandleStateChanged;
            _game.ZoneChanged += HandleZoneChanged;
            _game.SpinStarted += HandleSpinStarted;
            _game.RewardGained += HandleRewardGained;
            _wallet.BalanceChanged += HandleBalanceChanged;

            _views.Wheel.SpinClicked += HandleSpinClicked;
            _views.LeaveButton.Clicked += HandleLeaveClicked;
            _views.BombPopup.GiveUpClicked += HandleGiveUpClicked;
            _views.BombPopup.ReviveClicked += HandleReviveClicked;
            _views.SummaryPopup.PlayAgainClicked += HandlePlayAgainClicked;
        }

        private void Unsubscribe()
        {
            _game.StateChanged -= HandleStateChanged;
            _game.ZoneChanged -= HandleZoneChanged;
            _game.SpinStarted -= HandleSpinStarted;
            _game.RewardGained -= HandleRewardGained;
            _wallet.BalanceChanged -= HandleBalanceChanged;

            _views.Wheel.SpinClicked -= HandleSpinClicked;
            _views.LeaveButton.Clicked -= HandleLeaveClicked;
            _views.BombPopup.GiveUpClicked -= HandleGiveUpClicked;
            _views.BombPopup.ReviveClicked -= HandleReviveClicked;
            _views.SummaryPopup.PlayAgainClicked -= HandlePlayAgainClicked;
        }

        private void HandleStateChanged(GameState state)
        {
            GameState previous = _lastState;
            _lastState = state;

            _views.Wheel.SetSpinInteractable(_game.CanSpin);
            _views.LeaveButton.SetAvailable(_game.CanLeave);

            switch (state)
            {
                case GameState.Ready when previous == GameState.Bombed:
                    _views.BombPopup.Hide();
                    break;
                case GameState.Ready when previous == GameState.Lost || previous == GameState.Collected:
                    _views.SummaryPopup.Hide();
                    ClearCollectedRewards();
                    break;
                case GameState.Bombed:
                    _views.BombPopup.Show(_game.ReviveCost, _game.CanRevive);
                    break;
                case GameState.Lost:
                    _views.BombPopup.Hide();
                    ClearCollectedRewards();
                    _views.SummaryPopup.ShowLost(_texts.LostTitle, _texts.FormatLostInfo(_game.CurrentZone));
                    break;
                case GameState.Collected:
                    _views.SummaryPopup.ShowCollected(
                        _texts.CollectedTitle,
                        _texts.FormatCollectedInfo(_game.CurrentZone),
                        _rewardMapper.Map(_game.CollectedRewards));
                    break;
            }
        }

        private void HandleZoneChanged(int zone)
        {
            RenderZone();
        }

        private void RenderZone()
        {
            _views.ZoneBar.Render(_game.CurrentZone, _zoneRules);
            _views.Wheel.Render(_wheelViewData.Create(_game.CurrentZone, _game.CurrentWheel));
        }

        private void HandleSpinStarted(SpinResult result)
        {
            _landedSliceIndex = result.SliceIndex;
            _views.Wheel.Spin(result.SliceIndex, HandleSpinAnimationCompleted);
        }

        private void HandleSpinAnimationCompleted()
        {
            _game.TryCompleteSpin();
        }

        private void HandleRewardGained(RewardStack gained)
        {
            foreach (RewardStack stack in _game.CollectedRewards)
            {
                if (stack.RewardId == gained.RewardId)
                {
                    FlyToCollectedRewards(_rewardMapper.Map(stack), gained.Amount);
                    return;
                }
            }
        }

        // A new entry starts at zero so its counter runs up when the flying icons land.
        private void FlyToCollectedRewards(RewardEntryData total, int gainedAmount)
        {
            IRewardListView list = _views.CollectedRewards;
            if (!list.TryGetIconPosition(total.RewardId, out _))
            {
                list.Upsert(new RewardEntryData(total.RewardId, total.Icon, 0), animate: false);
            }

            list.TryGetIconPosition(total.RewardId, out Vector3 target);
            Vector3 origin = _views.Wheel.GetSliceIconPosition(_landedSliceIndex);

            int run = _run;
            _views.RewardFly.Fly(total.Icon, gainedAmount, origin, target, () =>
            {
                if (run == _run)
                {
                    list.Upsert(total, animate: true);
                }
            });
        }

        private void ClearCollectedRewards()
        {
            _run++;
            _views.RewardFly.CancelAll();
            _views.CollectedRewards.Clear();
        }

        private void HandleBalanceChanged(int balance)
        {
            _views.Wallet.SetBalance(balance, animate: true);
        }

        private void HandleSpinClicked()
        {
            _game.TryStartSpin();
        }

        private void HandleLeaveClicked()
        {
            _game.TryLeave();
        }

        private void HandleGiveUpClicked()
        {
            _game.TryGiveUp();
        }

        private void HandleReviveClicked()
        {
            _game.TryRevive();
        }

        private void HandlePlayAgainClicked()
        {
            _game.TryRestart();
        }
    }
}
