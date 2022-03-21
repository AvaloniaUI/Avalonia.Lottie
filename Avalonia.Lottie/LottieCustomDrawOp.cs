using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Model.Layer;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Utilities;

#nullable enable

namespace Avalonia.Lottie
{
    internal class LottieCustomDrawOp : ICustomDrawOperation
    {
        private readonly LottieCanvas _lottieCanvas;
        private readonly CompositionLayer _compositionLayer;
        private readonly Rect _destRect;
        private readonly Matrix _matrix;

        public LottieCustomDrawOp(LottieCanvas lottieCanvas, CompositionLayer compositionLayer, Rect destRect,
            Matrix matrix)
        {
            _lottieCanvas = lottieCanvas;
            _compositionLayer = compositionLayer;
            _destRect = destRect;
            _matrix = matrix;
        }

        public void Dispose()
        {
        }

        public bool HitTest(Point p) => false;

        public void Render(IDrawingContextImpl context)
        {
            using (_lottieCanvas.CreateSession(_destRect, context))
            {
                _compositionLayer.Draw(_lottieCanvas, _matrix, 255);
            }
        }

        public Rect Bounds => _destRect;

        public bool Equals(ICustomDrawOperation? other) => false;
    }
}