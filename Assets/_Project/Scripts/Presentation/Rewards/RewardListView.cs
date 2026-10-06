using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Presentation.Rewards
{
    /// <summary>
    /// Pools <see cref="RewardEntryView"/> instances cloned from an inactive template child.
    /// </summary>
    public sealed class RewardListView : MonoBehaviour, IRewardListView
    {
        [SerializeField] private RectTransform _container;
        [SerializeField] private RewardEntryView _entryTemplate;
        [Tooltip("Optional. Scrolled to the newest entry when one is added.")]
        [SerializeField] private ScrollRect _scrollRect;

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

        public bool TryGetIconPosition(string rewardId, out Vector3 worldPosition)
        {
            RewardEntryView view = Find(rewardId);
            worldPosition = view != null ? view.IconTransform.position : Vector3.zero;
            return view != null;
        }

        /// <summary>Pops the active entries in one after another.</summary>
        public void PlayAppear(float startDelay, float stagger)
        {
            for (int i = 0; i < _active.Count; i++)
            {
                _active[i].PlayAppear(startDelay + i * stagger);
            }
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

            // Positions must be valid this frame so effects can target the new entry.
            LayoutRebuilder.ForceRebuildLayoutImmediate(_container);
            if (_scrollRect != null)
            {
                _scrollRect.verticalNormalizedPosition = 0f;
            }

            return view;
        }
    }
}
