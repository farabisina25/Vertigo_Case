using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Presentation.Common;

namespace Vertigo.Wheel.Presentation.Hud
{
    /// <summary>
    /// "Collect rewards" button, only usable on safe and super zones while the wheel is idle.
    /// </summary>
    public sealed class LeaveButtonView : MonoBehaviour, ILeaveButtonView
    {
        private const string LeaveButtonName = "ui_button_leave";

        [SerializeField] private Button _leaveButton;
        [SerializeField] private CanvasGroup _animGroup;
        [Range(0f, 1f)] [SerializeField] private float _unavailableAlpha = 0.4f;
        [Min(0f)] [SerializeField] private float _fadeDuration = 0.2f;

        public event Action Clicked;

        private void Awake()
        {
            _leaveButton.onClick.AddListener(HandleClicked);
        }

        private void OnDestroy()
        {
            _leaveButton.onClick.RemoveListener(HandleClicked);
        }

        public void SetAvailable(bool available)
        {
            bool becameAvailable = available && !_leaveButton.interactable;
            _leaveButton.interactable = available;
            _animGroup.DOKill();
            _animGroup.DOFade(available ? 1f : _unavailableAlpha, _fadeDuration).SetLink(gameObject);

            if (becameAvailable)
            {
                Transform anim = _animGroup.transform;
                anim.DOKill(true);
                anim.DOPunchScale(Vector3.one * 0.15f, 0.45f, 5, 0.6f).SetLink(gameObject);
            }
        }

        private void HandleClicked()
        {
            Clicked?.Invoke();
        }

        private void OnValidate()
        {
            this.AssignFromChildren(ref _leaveButton, LeaveButtonName);
        }
    }
}
