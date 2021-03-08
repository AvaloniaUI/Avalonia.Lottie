using Avalonia.Media;
using LottieSharp.Animation.Content;


namespace LottieSharp
{
    public abstract class ColorFilter
    {
        public abstract Brush Apply(BitmapCanvas dst, IBrush brush);
    }
}