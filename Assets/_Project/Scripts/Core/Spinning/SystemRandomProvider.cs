using System;

namespace Vertigo.Wheel.Core.Spinning
{
    public sealed class SystemRandomProvider : IRandomProvider
    {
        private readonly Random _random;

        public SystemRandomProvider()
        {
            _random = new Random();
        }

        public SystemRandomProvider(int seed)
        {
            _random = new Random(seed);
        }

        public double NextDouble()
        {
            return _random.NextDouble();
        }
    }
}
