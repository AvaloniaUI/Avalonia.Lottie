using Avalonia.Media;


namespace LottieSharp.Animation.Content
{
    internal abstract class Gradient : Shader
    {
        public abstract Brush GetBrush(RenderTarget renderTarget, byte alpha);
    }
}