using System;
using System.Collections.Generic;
using Vertigo.Wheel.Presentation.Rewards;

namespace Vertigo.Wheel.Presentation.Popups
{
    public interface IRunSummaryPopupView
    {
        event Action PlayAgainClicked;

        void Show(string title, string info, IReadOnlyList<RewardEntryData> rewards);

        void Hide();

        void HideImmediate();
    }
}
