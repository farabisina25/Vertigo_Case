using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Presentation.Wheel
{
    public sealed class WheelSliceView : MonoBehaviour
    {
        [SerializeField] private Image _iconValue;
        [SerializeField] private TMP_Text _amountValue;

        [Header("Icon layout")]
        [Tooltip("Reward icons share the slot with the amount text.")]
        [SerializeField] private Vector2 _rewardIconPosition;
        [SerializeField] private Vector2 _rewardIconSize = new Vector2(56f, 56f);
        [Tooltip("The bomb has no amount, so its icon fills the slot.")]
        [SerializeField] private Vector2 _bombIconPosition;
        [SerializeField] private Vector2 _bombIconSize = new Vector2(84f, 84f);

        public Vector3 IconPosition => _iconValue.rectTransform.position;

        public void Render(SliceViewData data)
        {
            _iconValue.sprite = data.Icon;
            _iconValue.preserveAspect = true;
            _amountValue.text = data.AmountText;
            _amountValue.gameObject.SetActive(!data.IsBomb);

            RectTransform icon = _iconValue.rectTransform;
            icon.anchoredPosition = data.IsBomb ? _bombIconPosition : _rewardIconPosition;
            icon.sizeDelta = data.IsBomb ? _bombIconSize : _rewardIconSize;
        }

        public void PlayLanded()
        {
            RectTransform icon = _iconValue.rectTransform;
            icon.DOKill(true);
            icon.DOPunchScale(Vector3.one * 0.4f, 0.4f, 6, 0.5f).SetLink(gameObject);
        }
    }
}
