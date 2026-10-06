using System;

namespace Vertigo.Wheel.Presentation.Wheel
{
    public interface IWheelView
    {
        event Action SpinClicked;

        void Render(WheelViewData data);

        void SetSpinInteractable(bool interactable);

        void Spin(int sliceIndex, Action onComplete);
    }
}
