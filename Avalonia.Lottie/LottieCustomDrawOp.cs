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
        private readonly Size _sourceSize;
        private readonly Rect _viewPort;
 
        public LottieCustomDrawOp(BitmapCanvas bitmapCanvas, CompositionLayer compositionLayer, Size sourceSize, Rect viewPort)
        {
            _bitmapCanvas = bitmapCanvas;
            _compositionLayer = compositionLayer;
            _sourceSize = sourceSize;
            _viewPort = viewPort;
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
                finalRenderSurface = context.CreateLayer(_sourceSize);

                if (finalRenderSurface is null)
                {
                    context.Clear(Colors.Aqua);
                    return;
                }

                using (var renderSurfaceCtx = finalRenderSurface.CreateDrawingContext(null))
                {
                    using (var session = _bitmapCanvas.CreateSession(_sourceSize, finalRenderSurface,
                       renderSurfaceCtx))
                    {
                        _bitmapCanvas.Clear(Colors.Transparent);

                        var  matrix = Matrix.CreateScale(_viewPort.Width / _sourceSize.Width, _viewPort.Height / _sourceSize.Height);
                        
                        _compositionLayer.Draw(_bitmapCanvas, matrix, 255);
                    }
                }

                context.DrawBitmap(RefCountable.Create(finalRenderSurface),
                    1,
                    new Rect(_sourceSize), new Rect(_viewPort.Size));

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

        public Rect Bounds => _viewPort;

        public bool Equals(ICustomDrawOperation? other) => false;
    }
}