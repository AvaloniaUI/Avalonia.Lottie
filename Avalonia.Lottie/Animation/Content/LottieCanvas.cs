using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Utilities;
using Avalonia.Visuals.Media.Imaging;
using Newtonsoft.Json;

namespace Avalonia.Lottie.Animation.Content
{
    public class LottieCanvas
    {
        private readonly Stack<RenderTargetSave> _renderTargetSaves = new();

  
        public LottieCanvas(double width, double height)
        {
            //OutputRenderTarget = renderTarget;
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
                0,
                new PorterDuffXfermode(PorterDuff.Mode.Clear),
                255);

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
                    CurrentDrawingContext.DrawRectangle(null, new ImmutablePen(brush,
                            paint.StrokeWidth,
                            lineCap: paint.StrokeCap,
                            lineJoin: paint.StrokeJoin,
                            miterLimit: paint.StrokeMiter),
                        rect);
                else
                    CurrentDrawingContext.DrawRectangle(brush, null,
                        rect);
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
            return System.Reactive.Disposables.Disposable.Empty;
            ;
            // somehow there's a bug here that swaps the Y coord and Height value...
            return CurrentDrawingContext.PushClip(new Rect(rect.X, rect.Height, rect.Width, rect.Y));
        }

        public IDisposable Concat(Matrix parentMatrix)
        {
            return CurrentDrawingContext.PushPreTransform(parentMatrix);
            
            // _matrix = MatrixExt.PreConcat(_matrix, parentMatrix);
        }

        public IDisposable Save()
        {
            return CurrentDrawingContext?.PushSetTransform(Matrix.Identity);
        }

        public IDisposable SaveLayer(Rect bounds, Paint paint)
        {
            var rendertarget = CurrentDrawingContext.PlatformImpl.CreateLayer(bounds.Size);
            var rts = new RenderTargetSave(rendertarget, new DrawingContext(rendertarget.CreateDrawingContext(null)),
                bounds.Size,
                paint.Flags,
                paint.Xfermode,
                paint.Xfermode != null ? (byte) 255 : paint.Alpha);

            _renderTargetSaves.Push(rts);

            rts.Context.PlatformImpl.Clear(Colors.Transparent);

            return new Disposable(() =>
            {
                var renderTargetSave = _renderTargetSaves.Pop();

                var source = new Rect(renderTargetSave.Layer.PixelSize.ToSize(1));
                var destination = new Rect(renderTargetSave.BitmapSize);

                BitmapBlendingMode blendingMode = BitmapBlendingMode.SourceOver;

                if (renderTargetSave.PaintXfermode != null)
                    blendingMode = renderTargetSave.PaintXfermode.Mode switch
                    {
                        PorterDuff.Mode.SrcAtop => BitmapBlendingMode.SourceAtop,
                        PorterDuff.Mode.DstOut => BitmapBlendingMode.DestinationOut,
                        PorterDuff.Mode.DstIn => BitmapBlendingMode.DestinationIn,
                        _ => blendingMode
                    };


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


        public void Restore()
        {
        }


        public void DrawBitmap(Bitmap bitmap, Rect src, Rect dst, Paint paint)
        {
            using (CurrentDrawingContext.PushOpacity(paint.Alpha / 255d))
                CurrentDrawingContext.DrawImage(bitmap, src, dst);
        }

        public void Clear(Color color)
        {
            CurrentDrawingContext.PlatformImpl.Clear(color);
        }

        public IDisposable Translate(double dx, double dy)
        {
             
            return CurrentDrawingContext.PushPreTransform(Matrix.CreateTranslation(dx,dy));
        }

        public void SetMatrix(Matrix matrix)
        {
            // _matrix = matrix;
        }

        public Rect DrawText(char character, Paint paint)
        {
            var brush = paint.Shader is Gradient gradient
                ? gradient.GetBrush(paint.Alpha)
                : new SolidColorBrush(paint.Color);
            var finalBrush = paint.ColorFilter?.Apply(this, brush) ?? brush;

            var text = new string(character, 1);

            var textLayout = new FormattedText
            {
                Text = text,
                Typeface = new Media.Typeface(paint.Typeface.FontFamily, paint.Typeface.Style, paint.Typeface.Weight),
                FontSize = paint.TextSize
            };

            CurrentDrawingContext.DrawText(finalBrush, new Point(0, 0), textLayout);
            return new Rect(0, 0, textLayout.Bounds.Width, textLayout.Bounds.Height);
        }


        private struct RenderTargetSave
        {
            public RenderTargetSave(IDrawingContextLayerImpl layer, DrawingContext layerCtx, Size bitmapSize,
                int paintFlags, PorterDuffXfermode paintXfermode,
                byte paintAlpha)
            {
                BitmapSize = bitmapSize;
                Layer = layer;
                Context = layerCtx;
                PaintFlags = paintFlags;
                PaintXfermode = paintXfermode;
                PaintAlpha = paintAlpha;
            }

            public Size BitmapSize { get; }
            public DrawingContext Context { get; }

            public IDrawingContextLayerImpl Layer { get; }
            public int PaintFlags { get; }
            public PorterDuffXfermode PaintXfermode { get; }
            public byte PaintAlpha { get; }

            public void Dispose()
            {
                Context?.Dispose();
                Layer?.Dispose();
            }
        }
    }
}