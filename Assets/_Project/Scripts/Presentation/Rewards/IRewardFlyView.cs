using System;
using UnityEngine;

namespace Vertigo.Wheel.Presentation.Rewards
{
    public interface IRewardFlyView
    {
        /// <summary>Flies copies of the icon between two world positions and calls back when the last one lands.</summary>
        void Fly(Sprite icon, Vector3 from, Vector3 to, Action onArrived);

        void CancelAll();
    }
}
