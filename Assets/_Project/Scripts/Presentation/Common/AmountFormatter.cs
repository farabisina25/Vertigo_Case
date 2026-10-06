using System;
using System.Globalization;

namespace Vertigo.Wheel.Presentation.Common
{
    public static class AmountFormatter
    {
        private const int Thousand = 1_000;
        private const int CompactThreshold = 10_000;
        private const int Million = 1_000_000;

        /// <summary>1234 -> "1234", 12345 -> "12.3K", 1234567 -> "1.2M". Values are floored, never rounded up.</summary>
        public static string Compact(int amount)
        {
            if (amount >= Million)
            {
                return FloorToTenth(amount, Million) + "M";
            }

            if (amount >= CompactThreshold)
            {
                return FloorToTenth(amount, Thousand) + "K";
            }

            return amount.ToString(CultureInfo.InvariantCulture);
        }

        public static string Multiplier(int amount)
        {
            return "x" + Compact(amount);
        }

        private static string FloorToTenth(int amount, int unit)
        {
            double value = Math.Floor(amount * 10d / unit) / 10d;
            return value.ToString("0.#", CultureInfo.InvariantCulture);
        }
    }
}
