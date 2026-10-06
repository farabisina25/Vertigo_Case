using System;
using UnityEngine;

namespace Vertigo.Wheel.Presentation.Wheel
{
    public interface IWheelView
    {
        event Action SpinClicked;

        void Render(WheelViewData data);

        void SetSpinInteractable(bool interactable);

        /// <summary>Spins to the slice, holds on it and then calls <paramref name="onComplete"/>.</summary>
        void Spin(int sliceIndex, Action onComplete);

        Vector3 GetSliceIconPosition(int sliceIndex);
    }
}
