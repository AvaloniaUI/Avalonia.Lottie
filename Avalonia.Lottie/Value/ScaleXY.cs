namespace Avalonia.Lottie.Value
{
    public class ScaleXy
    {
        internal ScaleXy(double sx, double  sy)
        {
            ScaleX = sx;
            ScaleY = sy;
        }

        internal ScaleXy() : this(1f, 1f)
        {
        }

        internal virtual double  ScaleX { get; }

        internal virtual double  ScaleY { get; }

        public override string ToString()
        {
            return ScaleX + "x" + ScaleY;
        }
    }
}