
using Avalonia;


namespace Avalonia.Lottie.Animation.Content
{
    internal interface IDrawingContent : IContent
    {
        void Draw(BitmapCanvas canvas, Matrix3X3 parentMatrix, byte alpha);
        void GetBounds(ref Rect outBounds, Matrix3X3 parentMatrix);
    }
}