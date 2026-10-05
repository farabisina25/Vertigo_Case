using System;

namespace Vertigo.Wheel.Core.Wheels
{
    public readonly struct WheelSlice
    {
        public string RewardId { get; }
        public int Amount { get; }
        public float Weight { get; }
        public bool IsBomb { get; }

        private WheelSlice(string rewardId, int amount, float weight, bool isBomb)
        {
            if (weight < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(weight), "Weight cannot be negative.");
            }

            RewardId = rewardId;
            Amount = amount;
            Weight = weight;
            IsBomb = isBomb;
        }

        public static WheelSlice Reward(string rewardId, int amount, float weight)
        {
            if (string.IsNullOrEmpty(rewardId))
            {
                throw new ArgumentException("Reward id is required.", nameof(rewardId));
            }

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Reward amount must be positive.");
            }

            return new WheelSlice(rewardId, amount, weight, false);
        }

        public static WheelSlice Bomb(float weight)
        {
            return new WheelSlice(string.Empty, 0, weight, true);
        }
    }
}
