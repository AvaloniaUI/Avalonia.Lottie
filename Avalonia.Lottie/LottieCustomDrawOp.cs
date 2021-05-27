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
            var finalRenderSurface = context.CreateLayer(_destRect.Size);

            if (finalRenderSurface is null)
            {
                context.Clear(Colors.Aqua);
                return;
            }

            using (var renderSurfaceCtx = finalRenderSurface.CreateDrawingContext(null))
            {
                using (_lottieCanvas.CreateSession(_destRect.Size, finalRenderSurface,
                    new DrawingContext(renderSurfaceCtx)))
                {
                    _compositionLayer.Draw(_lottieCanvas, _matrix, 255);
                }
            }

            context.DrawBitmap(RefCountable.Create(finalRenderSurface),
                1,
                new Rect(new Point(), finalRenderSurface.PixelSize.ToSize(1)), _destRect);

            finalRenderSurface.Dispose();
        }

        public Rect Bounds => _destRect;

        public bool Equals(ICustomDrawOperation? other) => false;
    }
}