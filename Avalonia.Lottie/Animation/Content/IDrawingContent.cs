namespace Avalonia.Lottie.Animation.Content
{
    internal interface IDrawingContent : IContent
    {
        void Draw(LottieCanvas canvas, Matrix parentMatrix, byte alpha);
        void GetBounds(ref Rect outBounds, Matrix parentMatrix);
    }
}