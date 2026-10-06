using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Presentation.Common;

namespace Vertigo.Wheel.Presentation.Popups
{
    public sealed class BombPopupView : PopupView, IBombPopupView
    {
        private const string GiveUpButtonName = "ui_button_give_up";
        private const string ReviveButtonName = "ui_button_revive";

        [SerializeField] private Button _giveUpButton;
        [SerializeField] private Button _reviveButton;
        [SerializeField] private TMP_Text _reviveCostValue;
        [Tooltip("Optional. Shaken when the popup opens.")]
        [SerializeField] private RectTransform _card;

        public event Action GiveUpClicked;
        public event Action ReviveClicked;

        private void Awake()
        {
            _giveUpButton.onClick.AddListener(HandleGiveUpClicked);
            _reviveButton.onClick.AddListener(HandleReviveClicked);
        }

        private void OnDestroy()
        {
            _giveUpButton.onClick.RemoveListener(HandleGiveUpClicked);
            _reviveButton.onClick.RemoveListener(HandleReviveClicked);
        }

        public void Show(int reviveCost, bool canRevive)
        {
            _reviveCostValue.text = AmountFormatter.Compact(reviveCost);
            _reviveButton.interactable = canRevive;
            ShowPopup();
            ShakeCard();
        }

        private void ShakeCard()
        {
            if (_card == null)
            {
                return;
            }

            _card.DOKill(true);
            _card.DOShakeRotation(0.5f, new Vector3(0f, 0f, 18f), 14, 90f)
                .SetDelay(0.15f)
                .SetLink(gameObject);
        }

        private void HandleGiveUpClicked()
        {
            GiveUpClicked?.Invoke();
        }

        private void HandleReviveClicked()
        {
            ReviveClicked?.Invoke();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            this.AssignFromChildren(ref _giveUpButton, GiveUpButtonName);
            this.AssignFromChildren(ref _reviveButton, ReviveButtonName);
        }
    }
}
