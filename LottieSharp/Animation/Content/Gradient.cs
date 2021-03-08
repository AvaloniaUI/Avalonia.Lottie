using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;


namespace LottieSharp.Animation.Content
{
    internal abstract class Gradient : Shader
    {
        public abstract IBrush GetBrush(byte alpha);
    }
}