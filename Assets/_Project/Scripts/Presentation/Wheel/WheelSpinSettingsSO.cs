using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Presentation.Wheel
{
    [CreateAssetMenu(fileName = "wheel_spin_settings", menuName = "Vertigo/Wheel/Wheel Spin Settings")]
    public sealed class WheelSpinSettingsSO : ScriptableObject
    {
        [Min(0.1f)] [SerializeField] private float _duration = 4f;
        [Min(0)] [SerializeField] private int _fullRotations = 5;
        [SerializeField] private Ease _ease = Ease.OutQuart;

        [Tooltip("How far from the slice centre the wheel may stop, as a fraction of half a slice.")]
        [Range(0f, 0.9f)] [SerializeField] private float _landingJitter = 0.6f;

        public float Duration => _duration;
        public int FullRotations => _fullRotations;
        public Ease Ease => _ease;
        public float LandingJitter => _landingJitter;
    }
}
