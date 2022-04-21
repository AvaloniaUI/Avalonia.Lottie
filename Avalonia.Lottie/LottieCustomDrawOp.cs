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
        private readonly Matrix _viewportMatrix;

        public LottieCustomDrawOp(LottieCanvas lottieCanvas, CompositionLayer compositionLayer, Rect destRect,
            Matrix viewportMatrix)
        {
            _lottieCanvas = lottieCanvas;
            _compositionLayer = compositionLayer;
            _destRect = destRect;
            _viewportMatrix = viewportMatrix;
        }

        public void Dispose()
        {
        }

        public bool HitTest(Point p) => false;

        public void Render(IDrawingContextImpl context)
        {
            var compRect =  new Rect();
            _compositionLayer.GetBounds(ref compRect, _compositionLayer.Matrix);
            var absXMatrix = -compRect.X;
            var absYMatrix = -compRect.Y;
            _compositionLayer.Matrix *= _viewportMatrix;
            _compositionLayer.Matrix *=  Matrix.CreateTranslation(absXMatrix, absYMatrix);

            _compositionLayer.GetBounds(ref compRect, _compositionLayer.Matrix);

            using (_lottieCanvas.CreateSession(compRect, _viewportMatrix, context))
            {
                _compositionLayer.Draw(_lottieCanvas, _viewportMatrix, 255);
            }
        }

        public Rect Bounds => _destRect;

        public bool Equals(ICustomDrawOperation? other) => false;
    }
}