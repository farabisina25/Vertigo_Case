using System;

namespace Vertigo.Wheel.Presentation.Popups
{
    public interface IBombPopupView
    {
        event Action GiveUpClicked;
        event Action ReviveClicked;

        void Show(int reviveCost, bool canRevive);

        void Hide();

        void HideImmediate();
    }
}
