using System;
using Vertigo.Wheel.Presentation.Hud;
using Vertigo.Wheel.Presentation.Popups;
using Vertigo.Wheel.Presentation.Rewards;
using Vertigo.Wheel.Presentation.Wheel;
using Vertigo.Wheel.Presentation.Zones;

namespace Vertigo.Wheel.Controllers.Game
{
    public sealed class WheelGameViews
    {
        public IWheelView Wheel { get; }
        public IZoneBarView ZoneBar { get; }
        public IRewardListView CollectedRewards { get; }
        public IBombPopupView BombPopup { get; }
        public IRunSummaryPopupView SummaryPopup { get; }
        public ILeaveButtonView LeaveButton { get; }
        public IWalletView Wallet { get; }
        public IRewardFlyView RewardFly { get; }

        public WheelGameViews(
            IWheelView wheel,
            IZoneBarView zoneBar,
            IRewardListView collectedRewards,
            IBombPopupView bombPopup,
            IRunSummaryPopupView summaryPopup,
            ILeaveButtonView leaveButton,
            IWalletView wallet,
            IRewardFlyView rewardFly)
        {
            Wheel = wheel ?? throw new ArgumentNullException(nameof(wheel));
            ZoneBar = zoneBar ?? throw new ArgumentNullException(nameof(zoneBar));
            CollectedRewards = collectedRewards ?? throw new ArgumentNullException(nameof(collectedRewards));
            BombPopup = bombPopup ?? throw new ArgumentNullException(nameof(bombPopup));
            SummaryPopup = summaryPopup ?? throw new ArgumentNullException(nameof(summaryPopup));
            LeaveButton = leaveButton ?? throw new ArgumentNullException(nameof(leaveButton));
            Wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            RewardFly = rewardFly ?? throw new ArgumentNullException(nameof(rewardFly));
        }
    }
}
