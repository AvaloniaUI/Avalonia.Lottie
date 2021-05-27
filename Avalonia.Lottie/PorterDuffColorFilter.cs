using Avalonia.Lottie.Animation.Content;
using Avalonia.Media;

namespace Avalonia.Lottie
{
    public abstract class PorterDuffColorFilter : ColorFilter
    {
        protected PorterDuffColorFilter(Color color, PorterDuff.Mode mode)
        {
            Color = color;
            Mode = mode;
        }

        public Color Color { get; }
        public PorterDuff.Mode Mode { get; }

        public override IBrush Apply(LottieCanvas dst, IBrush brush)
        {
            //var originalColor = Colors.White;
            //if (brush is CompositionColorBrush compositionColorBrush)
            //    originalColor = compositionColorBrush.Color;
            //TODO: passthrough the color filters for now.
            return brush;
        }
    }
}