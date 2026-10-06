using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Vertigo.Wheel.Presentation.Rewards
{
    /// <summary>
    /// Bursts a few pooled icon copies out of the wheel and pulls them into the reward list.
    /// </summary>
    public sealed class RewardFlyView : MonoBehaviour, IRewardFlyView
    {
        [SerializeField] private Image _iconTemplate;
        [Tooltip("Copies flown for amounts of at least this many; smaller amounts fly as one icon.")]
        [Min(1)] [SerializeField] private int _iconCount = 4;
        [Min(0f)] [SerializeField] private float _stagger = 0.06f;
        [Min(0f)] [SerializeField] private float _burstRadius = 70f;
        [Min(0f)] [SerializeField] private float _burstDuration = 0.2f;
        [Min(0f)] [SerializeField] private float _flyDuration = 0.45f;
        [SerializeField] private float _arrivalScale = 0.6f;

        private readonly Stack<Image> _pool = new Stack<Image>();
        private readonly List<Sequence> _running = new List<Sequence>();

        public void Fly(Sprite icon, int amount, Vector3 from, Vector3 to, Action onArrived)
        {
            int count = GetIconCount(amount, _iconCount);
            float burstRadius = count > 1 ? _burstRadius * transform.lossyScale.x : 0f;
            for (int i = 0; i < count; i++)
            {
                bool isLast = i == count - 1;
                Image image = Rent(icon);
                RectTransform rect = image.rectTransform;
                rect.position = from;
                rect.localScale = Vector3.zero;

                Vector3 burst = from + (Vector3)(Random.insideUnitCircle * burstRadius);
                Sequence sequence = DOTween.Sequence()
                    .AppendInterval(i * _stagger)
                    .Append(rect.DOScale(1f, _burstDuration).SetEase(Ease.OutBack))
                    .Join(rect.DOMove(burst, _burstDuration).SetEase(Ease.OutQuad))
                    .Append(rect.DOMove(to, _flyDuration).SetEase(Ease.InCubic))
                    .Join(rect.DOScale(_arrivalScale, _flyDuration).SetEase(Ease.InQuad))
                    .SetLink(gameObject);

                sequence.OnComplete(() =>
                {
                    _running.Remove(sequence);
                    Return(image);
                    if (isLast)
                    {
                        onArrived?.Invoke();
                    }
                });
                _running.Add(sequence);
            }
        }

        public static int GetIconCount(int amount, int burstCount)
        {
            return amount >= burstCount ? burstCount : 1;
        }

        public void CancelAll()
        {
            foreach (Sequence sequence in _running.ToArray())
            {
                sequence.Kill(complete: false);
            }

            _running.Clear();
            foreach (Image image in GetComponentsInChildren<Image>())
            {
                if (image != _iconTemplate)
                {
                    Return(image);
                }
            }
        }

        private Image Rent(Sprite icon)
        {
            Image image = _pool.Count > 0 ? _pool.Pop() : Instantiate(_iconTemplate, transform);
            image.sprite = icon;
            image.preserveAspect = true;
            image.gameObject.SetActive(true);
            image.transform.SetAsLastSibling();
            return image;
        }

        private void Return(Image image)
        {
            if (!image.gameObject.activeSelf)
            {
                return;
            }

            image.gameObject.SetActive(false);
            _pool.Push(image);
        }
    }
}
