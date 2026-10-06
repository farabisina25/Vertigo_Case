using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Presentation.Wheel
{
    [CreateAssetMenu(fileName = "wheel_spin_settings", menuName = "Vertigo/Wheel/Wheel Spin Settings")]
    public sealed class WheelSpinSettingsSO : ScriptableObject
    {
        [Header("Spin")]
        [Min(0.1f)] [SerializeField] private float _duration = 4f;
        [Min(0)] [SerializeField] private int _fullRotations = 5;
        [SerializeField] private Ease _ease = Ease.OutQuart;

        [Tooltip("How far from the slice centre the wheel may stop, as a fraction of half a slice.")]
        [Range(0f, 0.9f)] [SerializeField] private float _landingJitter = 0.6f;

        [Header("Feedback")]
        [Tooltip("Indicator kick each time a slice passes under it.")]
        [Range(0f, 45f)] [SerializeField] private float _tickAngle = 14f;
        [Min(0f)] [SerializeField] private float _tickDuration = 0.12f;
        [Tooltip("Pause on the landed slice before the result is applied.")]
        [Min(0f)] [SerializeField] private float _resultHold = 0.45f;
        [Min(0f)] [SerializeField] private float _wheelChangePunch = 0.12f;
        [Tooltip("Seconds per full turn of the glow behind the wheel. 0 disables it.")]
        [Min(0f)] [SerializeField] private float _glowTurnDuration = 12f;

        public float Duration => _duration;
        public int FullRotations => _fullRotations;
        public Ease Ease => _ease;
        public float LandingJitter => _landingJitter;
        public float TickAngle => _tickAngle;
        public float TickDuration => _tickDuration;
        public float ResultHold => _resultHold;
        public float WheelChangePunch => _wheelChangePunch;
        public float GlowTurnDuration => _glowTurnDuration;
    }
}
