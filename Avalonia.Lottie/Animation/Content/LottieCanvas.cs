using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Utilities;
using Avalonia.Visuals.Media.Imaging;

namespace Avalonia.Lottie.Animation.Content
{
    public class LottieCanvas
    {
        public LottieCanvas(double width, double height)
        {
            UpdateSize(width, height);
        }

        private Size _lastSize = Size.Empty;

        private IDrawingContextImpl mainDrawingContext;


        private DrawingContext CurrentDrawingContext => ContextStack.Peek();

        private DrawingContext _layerCtx;
        private Stack<DrawingContext> ContextStack = new();

        public double Width { get; private set; }
        public double Height { get; private set; }

        private void UpdateSize(double width, double height)
        {
            if (double.IsNaN(width) || double.IsNaN(height))
            {
                return;
            }

            Width = width;
            Height = height;
            var curSize = new Size(Width, Height);

            if (curSize != _lastSize)
            {
                _lastSize = curSize;
            }
        }


        internal IDisposable CreateSession(Rect rect, Matrix viewportMatrix, IDrawingContextImpl drawingSession)
        {
            mainDrawingContext = drawingSession;
            ContextStack.Push(new DrawingContext(mainDrawingContext));
            UpdateSize(rect.Width, rect.Height);

            return new Disposable(() => { ContextStack.Pop(); });
        }

        public void DrawRect(double x1, double y1, double x2, double y2, Paint paint)
        {
            DrawRect(new Rect(x1, y1, x2 - x1, y2 - y1), paint);
        }

        private void DrawRect(Rect rect, Paint paint)
        {
            var brush = new ImmutableSolidColorBrush(paint.Color);
            {
                if (paint.Style == Paint.PaintStyle.Stroke)
                {
                    CurrentDrawingContext.DrawRectangle(null, new ImmutablePen(brush,
                        paint.StrokeWidth,
                        lineCap: paint.StrokeCap,
                        lineJoin: paint.StrokeJoin,
                        miterLimit: paint.StrokeMiter), rect);
                }
                else
                {
                    CurrentDrawingContext.DrawRectangle(brush, null, rect);
                }
            }
        }

        public void DrawPath(Path path, Paint paint, bool fromMask = false)
        {
            var brush = paint.Shader is Gradient gradient
                ? gradient.GetBrush(paint.Alpha)
                : new ImmutableSolidColorBrush(paint.Color);

            var geometry = path.GetGeometry();
            if (paint.Style == Paint.PaintStyle.Stroke)
            {
                var pen = new ImmutablePen(brush,
                    paint.StrokeWidth,
                    lineCap: paint.StrokeCap,
                    lineJoin: paint.StrokeJoin,
                    miterLimit: paint.StrokeMiter);

                CurrentDrawingContext.DrawGeometry(null, pen, geometry);
            }

            else
            {
                CurrentDrawingContext.DrawGeometry(brush, null, geometry);
            }
        }

        public IDisposable ClipRect(Rect rect)
        {
            // somehow there's a bug here that swaps the Y coord and Height value...
            return CurrentDrawingContext.PushClip(new Rect(rect.X, rect.Height, rect.Width, rect.Y));
        }

        public IDisposable Concat(Matrix parentMatrix)
        {
            return CurrentDrawingContext.PushPreTransform(parentMatrix);
        }

        public IDisposable Save()
        {
            return CurrentDrawingContext?.PushSetTransform(Matrix.Identity);
        }

        public IDisposable CreateLayer(Rect boundsx, Paint paint)
        {
            if (boundsx.Width > 1500 || boundsx.Height > 1500)
            {
                LottieLog.Warn("Layer Size seems to exceed common sense");
            }

            var bounds = new Rect(boundsx.X, boundsx.Y, Math.Max(boundsx.Width, 1500), Math.Max(boundsx.Height, 1500));
            IDrawingContextLayerImpl rts;
            try
            {
                rts = mainDrawingContext.CreateLayer(bounds.Size);
            }
            catch (Exception e)
            {
                return System.Reactive.Disposables.Disposable.Empty;
            }

            var source = new Rect(rts.PixelSize.ToSize(1));
            var destination = new Rect(bounds.Size);

            ContextStack.Push(new DrawingContext(rts.CreateDrawingContext(null)));
            return new Disposable(() =>
            {
                var curDc = ContextStack.Pop();

                var blendingMode = BitmapBlendingMode.SourceOver;

                if (paint.Xfermode != null)
                {
                    blendingMode = paint.Xfermode.Mode switch
                    {
                        PorterDuff.Mode.SrcAtop => BitmapBlendingMode.SourceAtop,
                        PorterDuff.Mode.DstOut => BitmapBlendingMode.DestinationOut,
                        PorterDuff.Mode.DstIn => BitmapBlendingMode.DestinationIn,
                        PorterDuff.Mode.Clear => BitmapBlendingMode.Destination,
                        _ => blendingMode
                    };
                }

                CurrentDrawingContext.PlatformImpl.PushBitmapBlendMode(blendingMode);
                CurrentDrawingContext.PlatformImpl.DrawBitmap(RefCountable.CreateUnownedNotClonable(rts),
                    1,
                    source, destination);
                CurrentDrawingContext.PlatformImpl.PopBitmapBlendMode();

                rts.Dispose();
                curDc.Dispose();
            });
        }

        public void DrawBitmap(Bitmap bitmap, Rect src, Rect dst, Paint paint)
        {
            using (CurrentDrawingContext.PushOpacity(paint.Alpha / 255d))
            {
                CurrentDrawingContext.DrawImage(bitmap, src, dst);
            }
        }

        public IDisposable Translate(double dx, double dy)
        {
            return CurrentDrawingContext.PushPreTransform(Matrix.CreateTranslation(dx, dy));
        }

        public Rect DrawText(char character, Paint paint)
        {
            var brush = paint.Shader is Gradient gradient
                ? gradient.GetBrush(paint.Alpha)
                : new ImmutableSolidColorBrush(paint.Color);
            var finalBrush = paint.ColorFilter?.Apply(this, brush) ?? brush;

            var text = new string(character, 1);

            // var textLayout = new FormattedText
            // {
            //     Text = text,
            //     Typeface = new Typeface(paint.Typeface.FontFamily, paint.Typeface.Style, paint.Typeface.Weight),
            //     FontSize = paint.TextSize
            // };
            //
            // CurrentDrawingContext.DrawText(finalBrush, new Point(0, 0), textLayout);
            // return new Rect(0, 0, textLayout.Bounds.Width, textLayout.Bounds.Height);
            return Rect.Empty;
        }
    }
}