using System;
using System.Collections.Generic;
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

        public event Action PlayAgainClicked;

        private void Awake()
        {
            _playAgainButton.onClick.AddListener(HandlePlayAgainClicked);
        }

        private void OnDestroy()
        {
            _playAgainButton.onClick.RemoveListener(HandlePlayAgainClicked);
        }

        public void Show(string title, string info, IReadOnlyList<RewardEntryData> rewards)
        {
            _titleValue.text = title;
            _infoValue.text = info;
            _rewardList.SetEntries(rewards);
            ShowPopup();
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
