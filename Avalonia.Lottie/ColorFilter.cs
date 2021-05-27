using Avalonia.Lottie.Animation.Content;
using Avalonia.Media;

namespace Avalonia.Lottie
{
    public abstract class ColorFilter
    {
        public abstract IBrush Apply(LottieCanvas dst, IBrush brush);
    }
}