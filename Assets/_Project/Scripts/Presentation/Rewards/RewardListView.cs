using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.Wheel.Presentation.Rewards
{
    /// <summary>
    /// Pools <see cref="RewardEntryView"/> instances cloned from an inactive template child.
    /// </summary>
    public sealed class RewardListView : MonoBehaviour, IRewardListView
    {
        [SerializeField] private RectTransform _container;
        [SerializeField] private RewardEntryView _entryTemplate;

        private readonly List<RewardEntryView> _active = new List<RewardEntryView>();
        private readonly Stack<RewardEntryView> _pool = new Stack<RewardEntryView>();

        public void SetEntries(IReadOnlyList<RewardEntryData> entries)
        {
            Clear();

            foreach (RewardEntryData entry in entries)
            {
                AddEntry(entry);
            }
        }

        /// <summary>Adds a new entry or animates the existing one to the new total.</summary>
        public void Upsert(RewardEntryData entry, bool animate)
        {
            RewardEntryView view = Find(entry.RewardId);
            if (view == null)
            {
                AddEntry(entry);
            }
            else if (animate)
            {
                view.AnimateAmount(entry.Amount);
            }
            else
            {
                view.Render(entry);
            }
        }

        public RewardEntryView Find(string rewardId)
        {
            foreach (RewardEntryView view in _active)
            {
                if (view.RewardId == rewardId)
                {
                    return view;
                }
            }

            return null;
        }

        public void Clear()
        {
            foreach (RewardEntryView view in _active)
            {
                view.gameObject.SetActive(false);
                _pool.Push(view);
            }

            _active.Clear();
        }

        private RewardEntryView AddEntry(RewardEntryData entry)
        {
            RewardEntryView view = _pool.Count > 0 ? _pool.Pop() : Instantiate(_entryTemplate, _container);
            view.transform.SetAsLastSibling();
            view.gameObject.SetActive(true);
            view.Render(entry);
            _active.Add(view);
            return view;
        }
    }
}
