using System;

namespace Avalonia.Lottie
{
    internal class AccelerateDecelerateInterpolator : IInterpolator
    {
        public double  GetInterpolation(double f)
        {
            if (f < 0 || double .IsNaN(f))
                f = 0;
            if (f > 1)
                f = 1;
            return  (Math.Cos((f + 1) * Math.PI) / 2 + 0.5);
        }
    }
}