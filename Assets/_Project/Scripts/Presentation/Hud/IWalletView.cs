using UnityEngine;

namespace Vertigo.Wheel.Presentation.Hud
{
    public interface IWalletView
    {
        void SetCurrencyIcon(Sprite icon);

        void SetBalance(int balance, bool animate);
    }
}
