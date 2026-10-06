using System;
using UnityEngine;

namespace Vertigo.Wheel.Presentation.Rewards
{
    public interface IRewardFlyView
    {
        /// <summary>
        /// Flies the icon between two world positions and calls back when the last copy lands.
        /// Small amounts fly as a single icon, larger ones burst into several copies.
        /// </summary>
        void Fly(Sprite icon, int amount, Vector3 from, Vector3 to, Action onArrived);

        void CancelAll();
    }
}
