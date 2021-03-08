using Avalonia.Media;
using LottieSharp.Animation.Content;


namespace LottieSharp
{
    public abstract class PathEffect
    {
        public abstract void Apply(DashStyle StrokeStyle, Paint paint);
    }
}