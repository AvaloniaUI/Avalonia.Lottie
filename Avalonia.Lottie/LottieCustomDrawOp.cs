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
        private readonly BitmapCanvas _bitmapCanvas;
        private readonly CompositionLayer _compositionLayer;
        private readonly Matrix _matrix;
        private readonly byte _alpha;
        private readonly Rect _sourceRect;
        private readonly Rect _destRect;
        private readonly bool _isResized;
        private readonly PixelSize _priorSize;
        private readonly double _renderScaling;
        private readonly Size _surfaceSize;

        public LottieCustomDrawOp(BitmapCanvas bitmapCanvas,
            CompositionLayer compositionLayer,
            Matrix matrix,
            byte alpha,
            Rect sourceRect,
            Rect destRect,
            bool isResized,
            PixelSize priorSize,
            double renderScaling,
            Size surfaceSize)
        {
            _bitmapCanvas = bitmapCanvas;
            _compositionLayer = compositionLayer;
            _matrix = matrix;
            _alpha = alpha;
            _sourceRect = sourceRect;
            _destRect = destRect;
            _isResized = isResized;
            _priorSize = priorSize;
            _renderScaling = renderScaling;
            _surfaceSize = surfaceSize;
        }

        public void Dispose()
        {
        }

        public bool HitTest(Point p) => false;

        public void Render(IDrawingContextImpl context)
        {
            IDrawingContextLayerImpl finalRenderSurface = null;
            try
            {
                finalRenderSurface = context.CreateLayer(_sourceRect.Size);

                using (var renderSurfaceCtx = finalRenderSurface.CreateDrawingContext(null))
                {
                    using (var session = _bitmapCanvas.CreateSession(_surfaceSize.Width,
                        _surfaceSize.Height, renderSurfaceCtx))
                    {
                        _bitmapCanvas.Clear(Colors.Transparent);
                        _compositionLayer.Draw(_bitmapCanvas, Matrix.Identity, _alpha);
                    }
                }

                context.DrawBitmap(RefCountable.Create(finalRenderSurface),
                    _alpha / 255d,
                    _sourceRect, _destRect);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
                context.Clear(Colors.Red);
            }
            finally
            {
                finalRenderSurface?.Dispose();
            }
        }

        public Rect Bounds => _destRect;

        public bool Equals(ICustomDrawOperation? other) => false;
    }
}