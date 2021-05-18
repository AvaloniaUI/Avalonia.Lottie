namespace Avalonia.Lottie.Animation.Content
{
    internal interface IDrawingContent : IContent
    {
        void Draw(BitmapCanvas canvas, Matrix parentMatrix, byte alpha);
        void GetBounds(ref Rect outBounds, Matrix parentMatrix);
    }
}