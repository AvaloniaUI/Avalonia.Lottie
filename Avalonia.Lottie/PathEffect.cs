using Avalonia.Lottie.Animation.Content;
using Avalonia.Media;


namespace Avalonia.Lottie
{
    public abstract class PathEffect
    {
        public abstract void Apply(DashStyle StrokeStyle, Paint paint);
    }
}