using System;

namespace Vertigo.Wheel.Presentation.Hud
{
    public interface ILeaveButtonView
    {
        event Action Clicked;

        void SetAvailable(bool available);
    }
}
