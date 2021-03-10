using Avalonia.Media;

namespace Avalonia.Lottie.Animation.Content
{
    internal abstract class Gradient : Shader
    {
        public abstract IBrush GetBrush(byte alpha);
    }
}