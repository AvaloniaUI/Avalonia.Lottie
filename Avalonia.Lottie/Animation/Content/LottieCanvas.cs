using System;
using System.Collections.Generic;
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
        private readonly Stack<RenderTargetSave> _renderTargetSaves = new();

        public LottieCanvas(double width, double height)
        {
            UpdateSize(width, height);
        }

        private DrawingContext CurrentDrawingContext =>
            _renderTargetSaves.Count > 0 ? _renderTargetSaves.Peek().Context : null;

        public double Width { get; private set; }
        public double Height { get; private set; }

        private void UpdateSize(double width, double height)
        {
            Width = width;
            Height = height;
        }

        internal IDisposable CreateSession(Size size, IDrawingContextLayerImpl layer,
            DrawingContext drawingSession)
        {
            UpdateSize(size.Width, size.Height);

            var rts = new RenderTargetSave(layer,
                drawingSession, size,
                new PorterDuffXfermode(PorterDuff.Mode.Clear));

            _renderTargetSaves.Push(rts);

            return new Disposable(() => { _renderTargetSaves.Clear(); });
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

        public IDisposable SaveLayer(Rect bounds, Paint paint)
        {
            var renderTarget = CurrentDrawingContext.PlatformImpl.CreateLayer(bounds.Size);
            var rts = new RenderTargetSave(renderTarget, new DrawingContext(renderTarget.CreateDrawingContext(null)),
                bounds.Size,
                paint.Xfermode);

            _renderTargetSaves.Push(rts);

            rts.Context.PlatformImpl.Clear(Colors.Transparent);

            return new Disposable(() =>
            {
                var renderTargetSave = _renderTargetSaves.Pop();
                var source = new Rect(renderTargetSave.Layer.PixelSize.ToSize(1));
                var destination = new Rect(renderTargetSave.BitmapSize);
                var blendingMode = BitmapBlendingMode.SourceOver;

                if (renderTargetSave.PaintTransferMode != null)
                {
                    blendingMode = renderTargetSave.PaintTransferMode.Mode switch
                    {
                        PorterDuff.Mode.SrcAtop => BitmapBlendingMode.SourceAtop,
                        PorterDuff.Mode.DstOut => BitmapBlendingMode.DestinationOut,
                        PorterDuff.Mode.DstIn => BitmapBlendingMode.DestinationIn,
                        _ => blendingMode
                    };
                }

                using (CurrentDrawingContext.PushSetTransform(Matrix.Identity))
                {
                    CurrentDrawingContext.PlatformImpl.PushBitmapBlendMode(blendingMode);
                    CurrentDrawingContext.PlatformImpl.DrawBitmap(RefCountable.Create(renderTargetSave.Layer), 1,
                        source, destination);
                    CurrentDrawingContext.PlatformImpl.PopBitmapBlendMode();
                }

                renderTargetSave.Dispose();
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

            var textLayout = new FormattedText
            {
                Text = text,
                Typeface = new Typeface(paint.Typeface.FontFamily, paint.Typeface.Style, paint.Typeface.Weight),
                FontSize = paint.TextSize
            };

            CurrentDrawingContext.DrawText(finalBrush, new Point(0, 0), textLayout);
            return new Rect(0, 0, textLayout.Bounds.Width, textLayout.Bounds.Height);
        }


        private readonly struct RenderTargetSave
        {
            public RenderTargetSave(IDrawingContextLayerImpl layer, DrawingContext layerCtx, Size bitmapSize,
                PorterDuffXfermode paintTransferMode)
            {
                BitmapSize = bitmapSize;
                Layer = layer;
                Context = layerCtx;
                PaintTransferMode = paintTransferMode;
            }

            public Size BitmapSize { get; }

            public DrawingContext Context { get; }

            public IDrawingContextLayerImpl Layer { get; }

            public PorterDuffXfermode PaintTransferMode { get; }

            public void Dispose()
            {
                Context?.Dispose();
                Layer?.Dispose();
            }
        }
    }
}