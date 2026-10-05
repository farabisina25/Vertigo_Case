using UnityEngine;
using Vertigo.Wheel.Data.Rewards;

namespace Vertigo.Wheel.Data.Settings
{
    [CreateAssetMenu(fileName = "game_settings", menuName = "Vertigo/Wheel/Game Settings")]
    public sealed class GameSettingsSO : ScriptableObject
    {
        [Header("Economy")]
        [Tooltip("Collected stacks of this reward are added to the wallet and used to pay for revives.")]
        [SerializeField] private RewardItemSO _walletCurrency;
        [Min(0)] [SerializeField] private int _startingBalance = 500;
        [SerializeField] private string _walletSaveKey = "vertigo_wheel_wallet";

        [Header("Revive")]
        [Min(0)] [SerializeField] private int _reviveBaseCost = 100;
        [Min(0)] [SerializeField] private int _reviveCostIncrease = 100;

        [Header("Randomness")]
        [SerializeField] private bool _useFixedSeed;
        [SerializeField] private int _seed;

        public RewardItemSO WalletCurrency => _walletCurrency;
        public int StartingBalance => _startingBalance;
        public string WalletSaveKey => _walletSaveKey;
        public int ReviveBaseCost => _reviveBaseCost;
        public int ReviveCostIncrease => _reviveCostIncrease;
        public bool UseFixedSeed => _useFixedSeed;
        public int Seed => _seed;

        private void OnValidate()
        {
            if (_walletCurrency == null)
            {
                Debug.LogWarning($"{name}: wallet currency is not assigned.", this);
            }
        }
    }
}
