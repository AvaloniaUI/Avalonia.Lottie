using System;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Model.Layer;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Utilities;
using Avalonia.VisualTree;

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

            animatedPoint = new Point(rnd.Next(0, (int) (_destRect.Width - animatedSize.Width)),
                rnd.Next(0, (int) (_destRect.Height - animatedSize.Height)));
        }

        public void Dispose()
        {
        }

        public bool HitTest(Point p) => false;

        private static int velocity = 1;
        private static Random rnd = new Random();

        private Point animatedPoint;
        private Size animatedSize = new Size(100, 100);
        private IPen immutPen = new ImmutablePen(Brushes.Coral);
        private Point vectorPoint = new Point(velocity, velocity);

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
                var sizeF = new Size(Bounds.Width, Bounds.Height);

                if (animatedPoint.X + animatedSize.Width >= sizeF.Width || animatedPoint.X < 0)
                {
                    vectorPoint = new Point(-vectorPoint.X, vectorPoint.Y);
                }

                if (animatedPoint.Y + animatedSize.Height >= sizeF.Height || animatedPoint.Y < 0)
                {
                    vectorPoint = new Point(vectorPoint.X, -vectorPoint.Y);
                }

                animatedPoint += vectorPoint;

                renderSurfaceCtx.DrawRectangle(Brushes.Red, null,
                    new RoundedRect(new Rect(animatedPoint, animatedSize)));

                renderSurfaceCtx.DrawRectangle(null, immutPen,
                    new RoundedRect(new Rect(sizeF)));
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