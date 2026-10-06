using System;
using System.Collections.Generic;
using Vertigo.Wheel.Presentation.Rewards;

namespace Vertigo.Wheel.Presentation.Popups
{
    public interface IRunSummaryPopupView
    {
        event Action PlayAgainClicked;

        void ShowCollected(string title, string info, IReadOnlyList<RewardEntryData> rewards);

        /// <summary>Shown after giving up: the death card takes the place of the reward list.</summary>
        void ShowLost(string title, string info);

        void Hide();

        void HideImmediate();
    }
}
