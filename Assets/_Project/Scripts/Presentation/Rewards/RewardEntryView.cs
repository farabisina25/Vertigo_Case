using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Presentation.Common;

namespace Vertigo.Wheel.Presentation.Rewards
{
    public sealed class RewardEntryView : MonoBehaviour
    {
        [SerializeField] private RectTransform _animRoot;
        [SerializeField] private Image _iconValue;
        [SerializeField] private TMP_Text _amountValue;
        [Min(0f)] [SerializeField] private float _countDuration = 0.4f;

        private CounterTween _counter;

        public string RewardId { get; private set; }
        public RectTransform IconTransform => _iconValue.rectTransform;

        private CounterTween Counter => _counter ??= new CounterTween(_amountValue, AmountFormatter.Multiplier);

        public void Render(RewardEntryData data)
        {
            RewardId = data.RewardId;
            _iconValue.sprite = data.Icon;
            _iconValue.preserveAspect = true;
            Counter.Set(data.Amount);
            _animRoot.localScale = Vector3.one;
        }

        public void AnimateAmount(int amount)
        {
            Counter.AnimateTo(amount, _countDuration, gameObject);

            _animRoot.DOKill(true);
            _animRoot.DOPunchScale(Vector3.one * 0.2f, 0.3f, 6, 0.6f).SetLink(gameObject);
        }
    }
}
