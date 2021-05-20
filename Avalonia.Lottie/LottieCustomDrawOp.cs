using System;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Model.Layer;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;

namespace Avalonia.Lottie
{
    internal readonly struct LottieCustomDrawOp : ICustomDrawOperation
    {
        private readonly BitmapCanvas _bitmapCanvas;
        private readonly CompositionLayer _compositionLayer;
        private readonly Matrix _matrix;
        private readonly byte _alpha;
        private readonly RenderTargetBitmap _renderTargetBitmap;
        private readonly Rect _sourceRect;
        private readonly Rect _destRect;

        public LottieCustomDrawOp(BitmapCanvas bitmapCanvas, CompositionLayer compositionLayer, Matrix matrix,
            byte alpha, RenderTargetBitmap renderTargetBitmap, Rect sourceRect, Rect destRect)
        {

            _bitmapCanvas = bitmapCanvas;
            _compositionLayer = compositionLayer;
            _matrix = matrix;
            _alpha = alpha;
            _renderTargetBitmap = renderTargetBitmap;
            _sourceRect = sourceRect;
            _destRect = destRect;
        }

        public void Dispose()
        {
        }

        public bool HitTest(Point p) => false;
        public void Render(IDrawingContextImpl context)
        {
            try
            {
                _compositionLayer.Draw(_bitmapCanvas, _matrix, _alpha);

                context.DrawBitmap(_renderTargetBitmap.PlatformImpl,_alpha/255d, _sourceRect, _destRect);

            }
            catch (Exception e)
            {
                context.Clear(Colors.Red);
            }
        }

        public Rect Bounds => _destRect;

        public bool Equals(ICustomDrawOperation? other) => false;
            
    }
}