using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Presentation.Common
{
    /// <summary>
    /// Fades and scales a separate animation child so the root transform is never animated.
    /// </summary>
    public abstract class PopupView : MonoBehaviour
    {
        [SerializeField] private RectTransform _animRoot;
        [SerializeField] private CanvasGroup _canvasGroup;
        [Min(0f)] [SerializeField] private float _showDuration = 0.3f;
        [Min(0f)] [SerializeField] private float _hideDuration = 0.2f;
        [SerializeField] private float _hiddenScale = 0.85f;

        private Sequence _sequence;

        public bool IsVisible { get; private set; }

        protected void ShowPopup()
        {
            IsVisible = true;
            gameObject.SetActive(true);
            KillSequence();

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = true;
            _animRoot.localScale = Vector3.one * _hiddenScale;

            _sequence = DOTween.Sequence()
                .Join(_canvasGroup.DOFade(1f, _showDuration))
                .Join(_animRoot.DOScale(1f, _showDuration).SetEase(Ease.OutBack))
                .OnComplete(() => _canvasGroup.interactable = true)
                .SetLink(gameObject);
        }

        public void Hide()
        {
            if (!IsVisible)
            {
                return;
            }

            IsVisible = false;
            KillSequence();
            _canvasGroup.interactable = false;

            _sequence = DOTween.Sequence()
                .Join(_canvasGroup.DOFade(0f, _hideDuration))
                .Join(_animRoot.DOScale(_hiddenScale, _hideDuration).SetEase(Ease.InQuad))
                .OnComplete(() => gameObject.SetActive(false))
                .SetLink(gameObject);
        }

        public void HideImmediate()
        {
            IsVisible = false;
            KillSequence();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            gameObject.SetActive(false);
        }

        private void KillSequence()
        {
            _sequence?.Kill();
            _sequence = null;
        }

        protected virtual void OnValidate()
        {
            if (_animRoot != null && _canvasGroup == null)
            {
                _canvasGroup = _animRoot.GetComponent<CanvasGroup>();
            }
        }
    }
}
