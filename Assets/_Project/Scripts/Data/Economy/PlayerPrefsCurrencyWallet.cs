using UnityEngine;
using Vertigo.Wheel.Core.Economy;

namespace Vertigo.Wheel.Data.Economy
{
    public sealed class PlayerPrefsCurrencyWallet : InMemoryCurrencyWallet
    {
        private readonly string _saveKey;

        public PlayerPrefsCurrencyWallet(string saveKey, int defaultBalance)
            : base(Mathf.Max(0, PlayerPrefs.GetInt(saveKey, defaultBalance)))
        {
            _saveKey = saveKey;
        }

        protected override void SetBalance(int balance)
        {
            base.SetBalance(balance);
            PlayerPrefs.SetInt(_saveKey, balance);
            PlayerPrefs.Save();
        }
    }
}
