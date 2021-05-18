using System;

namespace Avalonia.Lottie
{
    public static class MathExt
    {
        internal static double Hypot(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        public static double  Lerp(double value1, double  value2, double  amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public static double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}