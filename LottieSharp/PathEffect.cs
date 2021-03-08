using LottieSharp.Animation.Content;


namespace LottieSharp
{
    public abstract class PathEffect
    {
        public abstract void Apply(StrokeStyle StrokeStyle, Paint paint);
    }
}