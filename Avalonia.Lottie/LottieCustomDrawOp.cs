using System;
using System.Diagnostics;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Model.Layer;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Utilities;

namespace Avalonia.Lottie
{
    internal readonly struct LottieCustomDrawOp : ICustomDrawOperation
    {
        private readonly LottieCanvas _lottieCanvas;
        private readonly CompositionLayer _compositionLayer;
        private readonly Rect _bounds;
        private readonly Matrix _matrix;

        public LottieCustomDrawOp(LottieCanvas lottieCanvas, CompositionLayer compositionLayer, Rect bounds,
            Matrix matrix)
        {
            _lottieCanvas = lottieCanvas;
            _compositionLayer = compositionLayer;
            _bounds = bounds;
            _matrix = matrix;
        }

        public void Dispose()
        {
        }

        public bool HitTest(Point p) => false;

        public void Render(IDrawingContextImpl context)
        {
            IDrawingContextLayerImpl finalRenderSurface = null;
            finalRenderSurface = context.CreateLayer(_bounds.Size);

            if (finalRenderSurface is null)
            {
                context.Clear(Colors.Aqua);
                return;
            }

            using (var renderSurfaceCtx = finalRenderSurface.CreateDrawingContext(null))
            {
                using (_lottieCanvas.CreateSession(_bounds.Size, finalRenderSurface,
                    new DrawingContext(renderSurfaceCtx)))
                {
                    _compositionLayer.Draw(_lottieCanvas, _matrix, 255);
                }
            }

            context.DrawBitmap(RefCountable.Create(finalRenderSurface),
                1,
                new Rect(finalRenderSurface.PixelSize.ToSize(1)), _bounds);

            finalRenderSurface?.Dispose();
        }

        public Rect Bounds => _bounds;

        public bool Equals(ICustomDrawOperation? other) => false;
    }
}