using Avalonia.Lottie.Animation.Content;
using Avalonia.Media;


namespace Avalonia.Lottie
{
    public abstract class ColorFilter
    {
        public abstract IBrush Apply(BitmapCanvas dst, IBrush brush);
    }
}