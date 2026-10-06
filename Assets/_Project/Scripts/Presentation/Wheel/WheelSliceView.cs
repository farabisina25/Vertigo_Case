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
    }
}
