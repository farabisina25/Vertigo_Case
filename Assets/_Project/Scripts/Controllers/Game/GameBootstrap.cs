using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Controllers.Mapping;
using Vertigo.Wheel.Core.Economy;
using Vertigo.Wheel.Core.Game;
using Vertigo.Wheel.Core.Rewards;
using Vertigo.Wheel.Core.Spinning;
using Vertigo.Wheel.Core.Zones;
using Vertigo.Wheel.Data.Economy;
using Vertigo.Wheel.Data.Rewards;
using Vertigo.Wheel.Data.Settings;
using Vertigo.Wheel.Data.Wheels;
using Vertigo.Wheel.Presentation.Common;
using Vertigo.Wheel.Presentation.Hud;
using Vertigo.Wheel.Presentation.Popups;
using Vertigo.Wheel.Presentation.Rewards;
using Vertigo.Wheel.Presentation.Wheel;
using Vertigo.Wheel.Presentation.Zones;

namespace Vertigo.Wheel.Controllers.Game
{
    /// <summary>
    /// Composition root: builds the model from ScriptableObjects and wires it to the scene views.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const int TargetFrameRate = 60;

        [Header("Data")]
        [SerializeField] private ZoneProgressionSO _progression;
        [SerializeField] private RewardCatalogSO _rewardCatalog;
        [SerializeField] private GameSettingsSO _settings;
        [SerializeField] private GameTextsSO _texts;

        [Header("Views")]
        [SerializeField] private WheelView _wheelView;
        [SerializeField] private ZoneBarView _zoneBarView;
        [SerializeField] private RewardListView _collectedRewardsView;
        [SerializeField] private BombPopupView _bombPopupView;
        [SerializeField] private RunSummaryPopupView _summaryPopupView;
        [SerializeField] private LeaveButtonView _leaveButtonView;
        [SerializeField] private WalletView _walletView;
        [SerializeField] private RewardFlyView _rewardFlyView;

        private WheelGamePresenter _presenter;

        private void Awake()
        {
            Application.targetFrameRate = TargetFrameRate;
            DOTween.Init();
        }

        private void Start()
        {
            if (!HasAllReferences())
            {
                enabled = false;
                return;
            }

            var wallet = new PlayerPrefsCurrencyWallet(_settings.WalletSaveKey, _settings.StartingBalance);
            IZoneRules zoneRules = _progression.CreateZoneRules();
            IWheelGame game = CreateGame(zoneRules, wallet);

            _presenter = new WheelGamePresenter(
                game,
                zoneRules,
                wallet,
                new WheelViewDataFactory(_progression, _rewardCatalog),
                new RewardEntryMapper(_rewardCatalog),
                _texts,
                CreateViews());

            _presenter.Initialize(_settings.WalletCurrency.Id);
        }

        private void OnDestroy()
        {
            _presenter?.Dispose();
        }

        private IWheelGame CreateGame(IZoneRules zoneRules, ICurrencyWallet wallet)
        {
            IRandomProvider random = _settings.UseFixedSeed
                ? new SystemRandomProvider(_settings.Seed)
                : new SystemRandomProvider();

            return new WheelGameModel(
                zoneRules,
                new ScriptableWheelProvider(_progression),
                new WeightedSpinResolver(random),
                new RewardInventory(),
                new CurrencyReviveService(wallet, _settings.ReviveBaseCost, _settings.ReviveCostIncrease),
                new CurrencyClaimHandler(wallet, _settings.WalletCurrency.Id));
        }

        private WheelGameViews CreateViews()
        {
            return new WheelGameViews(
                _wheelView,
                _zoneBarView,
                _collectedRewardsView,
                _bombPopupView,
                _summaryPopupView,
                _leaveButtonView,
                _walletView,
                _rewardFlyView);
        }

        private bool HasAllReferences()
        {
            bool valid = true;
            valid &= Require(_progression, nameof(_progression));
            valid &= Require(_rewardCatalog, nameof(_rewardCatalog));
            valid &= Require(_settings, nameof(_settings));
            valid &= Require(_texts, nameof(_texts));
            valid &= Require(_wheelView, nameof(_wheelView));
            valid &= Require(_zoneBarView, nameof(_zoneBarView));
            valid &= Require(_collectedRewardsView, nameof(_collectedRewardsView));
            valid &= Require(_bombPopupView, nameof(_bombPopupView));
            valid &= Require(_summaryPopupView, nameof(_summaryPopupView));
            valid &= Require(_leaveButtonView, nameof(_leaveButtonView));
            valid &= Require(_walletView, nameof(_walletView));
            valid &= Require(_rewardFlyView, nameof(_rewardFlyView));

            if (_settings != null)
            {
                valid &= Require(_settings.WalletCurrency, "_settings.WalletCurrency");
            }

            return valid;
        }

        private bool Require(Object reference, string fieldName)
        {
            if (reference != null)
            {
                return true;
            }

            Debug.LogError($"{name}: {fieldName} is not assigned.", this);
            return false;
        }
    }
}
