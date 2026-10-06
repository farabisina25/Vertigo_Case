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

        public void Render(SliceViewData data)
        {
            _iconValue.sprite = data.Icon;
            _iconValue.preserveAspect = true;
            _amountValue.text = data.AmountText;
            _amountValue.gameObject.SetActive(!data.IsBomb);
        }

        public Vector3 IconPosition => _iconValue.rectTransform.position;

        public void PlayLanded()
        {
            RectTransform icon = _iconValue.rectTransform;
            icon.DOKill(true);
            icon.DOPunchScale(Vector3.one * 0.4f, 0.4f, 6, 0.5f).SetLink(gameObject);
        }
    }
}
