using UnityEngine;

namespace Vertigo.Wheel.Data.Rewards
{
    [CreateAssetMenu(fileName = "reward_", menuName = "Vertigo/Wheel/Reward Item")]
    public sealed class RewardItemSO : ScriptableObject
    {
        [Tooltip("Unique key used by the game logic. Defaults to the asset name.")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private RewardType _type;
        [SerializeField] private Sprite _icon;

        public string Id => _id;
        public string DisplayName => _displayName;
        public RewardType Type => _type;
        public Sprite Icon => _icon;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_id))
            {
                _id = name;
            }

            if (string.IsNullOrWhiteSpace(_displayName))
            {
                _displayName = name;
            }
        }
    }
}
