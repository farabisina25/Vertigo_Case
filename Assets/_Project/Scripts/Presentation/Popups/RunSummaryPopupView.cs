using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Presentation.Common;
using Vertigo.Wheel.Presentation.Rewards;

namespace Vertigo.Wheel.Presentation.Popups
{
    /// <summary>
    /// Shown when a run ends, either by collecting the rewards or by giving up after a bomb.
    /// </summary>
    public sealed class RunSummaryPopupView : PopupView, IRunSummaryPopupView
    {
        private const string PlayAgainButtonName = "ui_button_play_again";

        [SerializeField] private Button _playAgainButton;
        [SerializeField] private TMP_Text _titleValue;
        [SerializeField] private TMP_Text _infoValue;
        [SerializeField] private RewardListView _rewardList;
        [Tooltip("Death card shown instead of the reward list when the run was lost.")]
        [SerializeField] private RectTransform _lostCard;
        [Min(0f)] [SerializeField] private float _rewardsAppearDelay = 0.25f;
        [Min(0f)] [SerializeField] private float _rewardsAppearStagger = 0.08f;

        public event Action PlayAgainClicked;

        private void Awake()
        {
            _playAgainButton.onClick.AddListener(HandlePlayAgainClicked);
        }

        private void OnDestroy()
        {
            _playAgainButton.onClick.RemoveListener(HandlePlayAgainClicked);
        }

        public void ShowCollected(string title, string info, IReadOnlyList<RewardEntryData> rewards)
        {
            SetTexts(title, info);
            _lostCard.gameObject.SetActive(false);
            _rewardList.gameObject.SetActive(true);

            ShowPopup();
            _rewardList.SetEntries(rewards);
            _rewardList.PlayAppear(_rewardsAppearDelay, _rewardsAppearStagger);
        }

        public void ShowLost(string title, string info)
        {
            SetTexts(title, info);
            _rewardList.Clear();
            _rewardList.gameObject.SetActive(false);
            _lostCard.gameObject.SetActive(true);

            ShowPopup();
            _lostCard.DOKill(true);
            _lostCard.localScale = Vector3.zero;
            _lostCard.DOScale(1f, 0.35f)
                .SetDelay(_rewardsAppearDelay)
                .SetEase(Ease.OutBack)
                .SetLink(gameObject);
        }

        private void SetTexts(string title, string info)
        {
            _titleValue.text = title;
            _infoValue.text = info;
        }

        private void HandlePlayAgainClicked()
        {
            PlayAgainClicked?.Invoke();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            this.AssignFromChildren(ref _playAgainButton, PlayAgainButtonName);
        }
    }
}
