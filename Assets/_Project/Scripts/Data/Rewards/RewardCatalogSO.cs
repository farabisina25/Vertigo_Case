using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.Wheel.Data.Rewards
{
    [CreateAssetMenu(fileName = "reward_catalog", menuName = "Vertigo/Wheel/Reward Catalog")]
    public sealed class RewardCatalogSO : ScriptableObject, IRewardCatalog
    {
        [SerializeField] private List<RewardItemSO> _rewards = new List<RewardItemSO>();

        private Dictionary<string, RewardItemSO> _lookup;

        public IReadOnlyList<RewardItemSO> Rewards => _rewards;

        public bool TryGet(string rewardId, out RewardItemSO reward)
        {
            if (_lookup == null)
            {
                BuildLookup();
            }

            return _lookup.TryGetValue(rewardId, out reward);
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, RewardItemSO>(_rewards.Count);

            foreach (RewardItemSO reward in _rewards)
            {
                if (reward == null)
                {
                    continue;
                }

                if (_lookup.ContainsKey(reward.Id))
                {
                    Debug.LogError($"Duplicate reward id '{reward.Id}' in {name}.", this);
                    continue;
                }

                _lookup.Add(reward.Id, reward);
            }
        }

        private void OnValidate()
        {
            _lookup = null;
            BuildLookup();
        }
    }
}
