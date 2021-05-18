namespace Avalonia.Lottie
{
    public class LinearInterpolator : IInterpolator
    {
        public double  GetInterpolation(double f)
        {
            if (f < 0 || double .IsNaN(f))
                f = 0;
            if (f > 1)
                f = 1;
            return f;
        }
    }
}