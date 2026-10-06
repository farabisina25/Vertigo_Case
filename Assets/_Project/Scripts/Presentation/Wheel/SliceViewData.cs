using UnityEngine;

namespace Vertigo.Wheel.Presentation.Wheel
{
    public readonly struct SliceViewData
    {
        public Sprite Icon { get; }
        public string AmountText { get; }
        public bool IsBomb { get; }

        public SliceViewData(Sprite icon, string amountText, bool isBomb)
        {
            Icon = icon;
            AmountText = amountText;
            IsBomb = isBomb;
        }
    }
}
