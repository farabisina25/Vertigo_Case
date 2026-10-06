using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Presentation.Common;

namespace Vertigo.Wheel.Presentation.Hud
{
    public sealed class WalletView : MonoBehaviour
    {
        [SerializeField] private Image _currencyIconValue;
        [SerializeField] private TMP_Text _balanceValue;
        [Min(0f)] [SerializeField] private float _countDuration = 0.5f;

        private CounterTween _counter;

        private CounterTween Counter => _counter ??= new CounterTween(_balanceValue, AmountFormatter.Compact);

        public void SetCurrencyIcon(Sprite icon)
        {
            _currencyIconValue.sprite = icon;
            _currencyIconValue.preserveAspect = true;
        }

        public void SetBalance(int balance, bool animate)
        {
            if (animate)
            {
                Counter.AnimateTo(balance, _countDuration, gameObject);
            }
            else
            {
                Counter.Set(balance);
            }
        }
    }
}
