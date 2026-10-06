using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Vertigo.Wheel.Presentation.Common
{
    /// <summary>
    /// Animates a number shown in a text field, remembering the value currently displayed.
    /// </summary>
    public sealed class CounterTween
    {
        private readonly TMP_Text _text;
        private readonly Func<int, string> _format;
        private Tween _tween;
        private int _displayed;

        public CounterTween(TMP_Text text, Func<int, string> format)
        {
            _text = text;
            _format = format;
        }

        public void Set(int value)
        {
            _tween?.Kill();
            Apply(value);
        }

        public void AnimateTo(int value, float duration, GameObject link)
        {
            _tween?.Kill();
            _tween = DOTween.To(() => _displayed, Apply, value, duration)
                .SetEase(Ease.OutCubic)
                .SetLink(link);
        }

        private void Apply(int value)
        {
            _displayed = value;
            _text.text = _format(value);
        }
    }
}
