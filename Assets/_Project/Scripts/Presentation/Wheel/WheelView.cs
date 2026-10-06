using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Presentation.Common;
using Random = UnityEngine.Random;

namespace Vertigo.Wheel.Presentation.Wheel
{
    public sealed class WheelView : MonoBehaviour, IWheelView
    {
        private const string SpinButtonName = "ui_button_spin";

        [SerializeField] private Button _spinButton;
        [SerializeField] private RectTransform _rotationRoot;
        [SerializeField] private Image _baseValue;
        [SerializeField] private Image _indicatorValue;
        [SerializeField] private TMP_Text _titleValue;
        [SerializeField] private TMP_Text _subtitleValue;
        [SerializeField] private WheelSliceView[] _slices = Array.Empty<WheelSliceView>();
        [SerializeField] private WheelSpinSettingsSO _spinSettings;

        private Tween _spinTween;

        public event Action SpinClicked;

        public bool IsSpinning => _spinTween != null && _spinTween.IsActive();

        private void Awake()
        {
            _spinButton.onClick.AddListener(HandleSpinClicked);
        }

        private void OnDestroy()
        {
            _spinButton.onClick.RemoveListener(HandleSpinClicked);
        }

        public void Render(WheelViewData data)
        {
            _baseValue.sprite = data.BaseSprite;
            _indicatorValue.sprite = data.IndicatorSprite;
            _titleValue.text = data.Title;
            _subtitleValue.text = data.Subtitle;

            if (data.Slices.Count != _slices.Length)
            {
                Debug.LogError($"{name}: wheel has {_slices.Length} slice views but {data.Slices.Count} slices.", this);
            }

            int count = Mathf.Min(data.Slices.Count, _slices.Length);
            for (int i = 0; i < count; i++)
            {
                _slices[i].Render(data.Slices[i]);
            }
        }

        public void SetSpinInteractable(bool interactable)
        {
            _spinButton.interactable = interactable;
        }

        public void Spin(int sliceIndex, Action onComplete)
        {
            _spinTween?.Kill();

            float targetAngle = GetTargetAngle(sliceIndex);
            _spinTween = _rotationRoot
                .DOLocalRotate(new Vector3(0f, 0f, targetAngle), _spinSettings.Duration, RotateMode.FastBeyond360)
                .SetEase(_spinSettings.Ease)
                .OnComplete(() =>
                {
                    _spinTween = null;
                    onComplete?.Invoke();
                })
                .SetLink(gameObject);
        }

        private float GetTargetAngle(int sliceIndex)
        {
            float halfSlice = WheelAngles.GetSliceAngle(_slices.Length) * 0.5f;
            float jitter = Random.Range(-1f, 1f) * _spinSettings.LandingJitter * halfSlice;

            return WheelAngles.GetClockwiseTarget(
                _rotationRoot.localEulerAngles.z,
                sliceIndex,
                _slices.Length,
                _spinSettings.FullRotations,
                jitter);
        }

        private void HandleSpinClicked()
        {
            SpinClicked?.Invoke();
        }

        private void OnValidate()
        {
            this.AssignFromChildren(ref _spinButton, SpinButtonName);
        }
    }
}
