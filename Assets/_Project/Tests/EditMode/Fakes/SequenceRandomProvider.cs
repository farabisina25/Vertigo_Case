using System.Collections.Generic;
using Vertigo.Wheel.Core.Spinning;

namespace Vertigo.Wheel.Tests.Fakes
{
    internal sealed class SequenceRandomProvider : IRandomProvider
    {
        private readonly Queue<double> _values;

        public SequenceRandomProvider(params double[] values)
        {
            _values = new Queue<double>(values);
        }

        public double NextDouble()
        {
            return _values.Dequeue();
        }
    }
}
