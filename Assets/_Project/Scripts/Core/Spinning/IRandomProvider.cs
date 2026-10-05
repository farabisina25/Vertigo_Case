namespace Vertigo.Wheel.Core.Spinning
{
    public interface IRandomProvider
    {
        /// <returns>A value in the range [0, 1).</returns>
        double NextDouble();
    }
}
