using System;
using System.Collections.Generic;
using UnityEngine;
using Vertigo.Wheel.Controllers.Mapping;
using Vertigo.Wheel.Core.Wheels;
using Vertigo.Wheel.Core.Zones;
using Vertigo.Wheel.Data.Rewards;
using Vertigo.Wheel.Presentation.Hud;
using Vertigo.Wheel.Presentation.Popups;
using Vertigo.Wheel.Presentation.Rewards;
using Vertigo.Wheel.Presentation.Wheel;
using Vertigo.Wheel.Presentation.Zones;

namespace Vertigo.Wheel.Tests.Fakes
{
    internal sealed class FakeWheelView : IWheelView
    {
        public event Action SpinClicked;

        public WheelViewData LastRender { get; private set; }
        public bool SpinInteractable { get; private set; }
        public int LastSpinIndex { get; private set; } = -1;
        public bool CompleteSpinsImmediately { get; set; } = true;
        public Action PendingCompletion { get; private set; }

        public void Render(WheelViewData data) => LastRender = data;
        public void SetSpinInteractable(bool interactable) => SpinInteractable = interactable;

        public void Spin(int sliceIndex, Action onComplete)
        {
            LastSpinIndex = sliceIndex;
            if (CompleteSpinsImmediately)
            {
                onComplete();
            }
            else
            {
                PendingCompletion = onComplete;
            }
        }

        public void ClickSpin() => SpinClicked?.Invoke();
    }

    internal sealed class FakeZoneBarView : IZoneBarView
    {
        public int LastZone { get; private set; }
        public void Render(int currentZone, IZoneRules zoneRules) => LastZone = currentZone;
    }

    internal sealed class FakeRewardListView : IRewardListView
    {
        public Dictionary<string, int> Amounts { get; } = new Dictionary<string, int>();

        public void SetEntries(IReadOnlyList<RewardEntryData> entries)
        {
            Amounts.Clear();
            foreach (RewardEntryData entry in entries)
            {
                Amounts[entry.RewardId] = entry.Amount;
            }
        }

        public void Upsert(RewardEntryData entry, bool animate) => Amounts[entry.RewardId] = entry.Amount;
        public void Clear() => Amounts.Clear();
    }

    internal sealed class FakeBombPopupView : IBombPopupView
    {
        public event Action GiveUpClicked;
        public event Action ReviveClicked;

        public bool Visible { get; private set; }
        public int ShownCost { get; private set; }
        public bool ShownCanRevive { get; private set; }

        public void Show(int reviveCost, bool canRevive)
        {
            Visible = true;
            ShownCost = reviveCost;
            ShownCanRevive = canRevive;
        }

        public void Hide() => Visible = false;
        public void HideImmediate() => Visible = false;
        public void ClickGiveUp() => GiveUpClicked?.Invoke();
        public void ClickRevive() => ReviveClicked?.Invoke();
    }

    internal sealed class FakeSummaryPopupView : IRunSummaryPopupView
    {
        public event Action PlayAgainClicked;

        public bool Visible { get; private set; }
        public string Title { get; private set; }
        public IReadOnlyList<RewardEntryData> Rewards { get; private set; }

        public void Show(string title, string info, IReadOnlyList<RewardEntryData> rewards)
        {
            Visible = true;
            Title = title;
            Rewards = rewards;
        }

        public void Hide() => Visible = false;
        public void HideImmediate() => Visible = false;
        public void ClickPlayAgain() => PlayAgainClicked?.Invoke();
    }

    internal sealed class FakeLeaveButtonView : ILeaveButtonView
    {
        public event Action Clicked;

        public bool Available { get; private set; }

        public void SetAvailable(bool available) => Available = available;
        public void Click() => Clicked?.Invoke();
    }

    internal sealed class FakeWalletView : IWalletView
    {
        public int Balance { get; private set; }

        public void SetCurrencyIcon(Sprite icon)
        {
        }

        public void SetBalance(int balance, bool animate) => Balance = balance;
    }

    internal sealed class FakeWheelViewDataProvider : IWheelViewDataProvider
    {
        public WheelViewData Create(int zone, WheelDefinition wheel)
        {
            var slices = new List<SliceViewData>();
            foreach (WheelSlice slice in wheel.Slices)
            {
                slices.Add(new SliceViewData(null, slice.Amount.ToString(), slice.IsBomb));
            }

            return new WheelViewData(null, null, wheel.ZoneType.ToString(), string.Empty, slices);
        }
    }

    internal sealed class EmptyRewardCatalog : IRewardCatalog
    {
        public bool TryGet(string rewardId, out RewardItemSO reward)
        {
            reward = null;
            return false;
        }
    }
}
