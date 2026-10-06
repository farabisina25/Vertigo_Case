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
        [Tooltip("Child scaled for feedback; the view root itself is never animated.")]
        [SerializeField] private RectTransform _animRoot;
        [SerializeField] private RectTransform _rotationRoot;
        [SerializeField] private RectTransform _glow;
        [SerializeField] private Image _baseValue;
        [SerializeField] private Image _indicatorValue;
        [SerializeField] private TMP_Text _titleValue;
        [SerializeField] private TMP_Text _subtitleValue;
        [SerializeField] private WheelSliceView[] _slices = Array.Empty<WheelSliceView>();
        [SerializeField] private WheelSpinSettingsSO _spinSettings;

        private Tween _spinTween;
        private int _tickSlice;
        private bool _hasRendered;

        public event Action SpinClicked;

        public bool IsSpinning => _spinTween != null && _spinTween.IsActive();

        private void Awake()
        {
            _spinButton.onClick.AddListener(HandleSpinClicked);
        }

        private void Start()
        {
            if (_glow != null && _spinSettings.GlowTurnDuration > 0f)
            {
                _glow.DOLocalRotate(new Vector3(0f, 0f, -WheelAngles.FullTurn), _spinSettings.GlowTurnDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart)
                    .SetLink(gameObject);
            }
        }

        private void OnDestroy()
        {
            _spinButton.onClick.RemoveListener(HandleSpinClicked);
        }

        public void Render(WheelViewData data)
        {
            bool wheelChanged = _hasRendered && _baseValue.sprite != data.BaseSprite;
            _hasRendered = true;

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

            if (wheelChanged)
            {
                _animRoot.DOKill(true);
                _animRoot.DOPunchScale(Vector3.one * _spinSettings.WheelChangePunch, 0.4f, 5, 0.6f).SetLink(gameObject);
            }
        }

        public void SetSpinInteractable(bool interactable)
        {
            _spinButton.interactable = interactable;
        }

        public Vector3 GetSliceIconPosition(int sliceIndex)
        {
            return _slices[sliceIndex].IconPosition;
        }

        public void Spin(int sliceIndex, Action onComplete)
        {
            _spinTween?.Kill();
            _tickSlice = GetSliceUnderIndicator();

            float targetAngle = GetTargetAngle(sliceIndex);
            _spinTween = DOTween.Sequence()
                .Append(_rotationRoot
                    .DOLocalRotate(new Vector3(0f, 0f, targetAngle), _spinSettings.Duration, RotateMode.FastBeyond360)
                    .SetEase(_spinSettings.Ease))
                .AppendCallback(() => _slices[sliceIndex].PlayLanded())
                .AppendInterval(_spinSettings.ResultHold)
                .OnUpdate(UpdateTick)
                .OnComplete(() =>
                {
                    _spinTween = null;
                    onComplete?.Invoke();
                })
                .SetLink(gameObject);
        }

        private void UpdateTick()
        {
            int slice = GetSliceUnderIndicator();
            if (slice == _tickSlice)
            {
                return;
            }

            _tickSlice = slice;
            RectTransform indicator = _indicatorValue.rectTransform;
            indicator.DOKill(true);
            indicator.DOPunchRotation(new Vector3(0f, 0f, _spinSettings.TickAngle), _spinSettings.TickDuration, 1, 0f)
                .SetLink(gameObject);
        }

        private int GetSliceUnderIndicator()
        {
            float sliceAngle = WheelAngles.GetSliceAngle(_slices.Length);
            return Mathf.FloorToInt((_rotationRoot.localEulerAngles.z + sliceAngle * 0.5f) / sliceAngle) % _slices.Length;
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
