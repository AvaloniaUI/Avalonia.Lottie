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
    public class LottieCanvas : IDisposable
    {
        public static int MatrixSaveFlag = 0b00001;

        public static int ClipSaveFlag = 0b00010;

        //public static int HasAlphaLayerSaveFlag = 0b00100;
        //public static int FullColorLayerSaveFlag = 0b01000;
        public static int ClipToLayerSaveFlag = 0b10000;
        public static int AllSaveFlag = 0b11111;

        private readonly Stack<ClipSave> _clipSaves = new();
        private readonly Stack<int> _flagSaves = new();
        private readonly Stack<Matrix> _matrixSaves = new();


        private readonly Stack<RenderTargetSave> _renderTargetSaves = new();
        private Rect _currentClip;
        private Matrix _matrix = Matrix.Identity;

        public LottieCanvas(double width, double height)
        {
            //OutputRenderTarget = renderTarget;
            UpdateClip(width, height);
        }

        internal IDrawingContextImpl CurrentDrawingContext =>
            _renderTargetSaves.Count > 0 ? _renderTargetSaves.Peek().Context : null;

        public double Width { get; private set; }
        public double Height { get; private set; }

        public void Dispose()
        {
            _renderTargetSaves.Clear();
        }

        private void UpdateClip(double width, double height)
        {
            if (Math.Abs(width - Width) > 0 || Math.Abs(height - Height) > 0)
            {
                Dispose();
            }

            Width = width;
            Height = height;
            _currentClip = new Rect(0, 0, Width, Height);
            
            
        }
        
        internal IDisposable CreateSession(Size size,IDrawingContextLayerImpl layer, IDrawingContextImpl drawingSession)
        {
            UpdateClip(size.Width, size.Height);
            
            _renderTargetSaves.Clear();

             var rts = new RenderTargetSave(layer,
                 drawingSession, size,
                0,
                new PorterDuffXfermode(PorterDuff.Mode.Clear),
                255);
             
            _renderTargetSaves.Push(rts);

            rts.Context.Clear(Colors.Transparent);

            

            return PushMask(_currentClip, 1f);
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

        public Disposable PushMask(Rect rect, double alpha, Path path = null)
        {
            if (alpha >= 1 && path == null)
            {
                CurrentDrawingContext.PushClip(rect);

                return new Disposable(() => { CurrentDrawingContext.PopClip(); });
            }

            var geometery = path.GetGeometry();

            CurrentDrawingContext.PushGeometryClip(geometery);

            return new Disposable(() => { CurrentDrawingContext.PopGeometryClip(); });
        }


        public bool ClipRect(Rect rect)
        {
            _currentClip.Intersects(rect);
            return _currentClip.IsEmpty == false;
        }

        public void Concat(Matrix parentMatrix)
        {
            _matrix = MatrixExt.PreConcat(_matrix, parentMatrix);
        }

        public void Save()
        {
            _flagSaves.Push(MatrixSaveFlag | ClipSaveFlag);
            SaveMatrix();
            SaveClip(255);
        }

        public void SaveLayer(Rect bounds, Paint paint, int flags, Path path = null)
        {
            _flagSaves.Push(flags);
            if ((flags & MatrixSaveFlag) == MatrixSaveFlag) SaveMatrix();

            var isClipToLayer = (flags & ClipToLayerSaveFlag) == ClipToLayerSaveFlag;

            if (isClipToLayer)
            {
                var rendertarget = CurrentDrawingContext.CreateLayer(bounds.Size);
                var rts = new RenderTargetSave(rendertarget, rendertarget.CreateDrawingContext(null), bounds.Size, paint.Flags,
                    paint.Xfermode,
                    paint.Xfermode != null ? (byte) 255 : paint.Alpha);

                _renderTargetSaves.Push(rts);

                rts.Context.Clear(Colors.Transparent);
            }

            if ((flags & ClipSaveFlag) == ClipSaveFlag) SaveClip(isClipToLayer ? (byte) 255 : paint.Alpha, path);
        }


        private void SaveMatrix()
        {
            var copy = new Matrix();
            copy = _matrix;
            _matrixSaves.Push(copy);
        }

        private void SaveClip(byte alpha, Path path = null)
        {
            var currentLayer = PushMask(_currentClip, alpha / 255f, path);

            _clipSaves.Push(new ClipSave(_currentClip, currentLayer));
        }

        public void RestoreAll()
        {
            while (_flagSaves.Count > 0)
                Restore();
        }

        public void Restore()
        {
            if (_flagSaves.Count < 1) return;

            var flags = _flagSaves.Pop();

            if ((flags & MatrixSaveFlag) == MatrixSaveFlag) _matrix = _matrixSaves.Pop();

            if ((flags & ClipSaveFlag) == ClipSaveFlag)
            {
                var clipSave = _clipSaves.Pop();
                _currentClip = clipSave.Rect;
                clipSave.Layer.Dispose();
            }

            if ((flags & ClipToLayerSaveFlag) == ClipToLayerSaveFlag)
            {
                var renderTargetSave = _renderTargetSaves.Pop();

                var j = new Rect(0, 0, renderTargetSave.BitmapSize.Width, renderTargetSave.BitmapSize.Height);

                BitmapBlendingMode blendingMode = BitmapBlendingMode.SourceOver;

                if (renderTargetSave.PaintXfermode != null)
                    blendingMode = renderTargetSave.PaintXfermode.Mode switch
                    {
                        PorterDuff.Mode.SrcAtop => BitmapBlendingMode.SourceAtop,
                        PorterDuff.Mode.DstOut => BitmapBlendingMode.DestinationOut,
                        PorterDuff.Mode.DstIn => BitmapBlendingMode.DestinationIn,
                        _ => blendingMode
                    };

                CurrentDrawingContext.PushBitmapBlendMode(blendingMode);
                CurrentDrawingContext.DrawBitmap(RefCountable.Create(renderTargetSave.Layer), 1, j, j);
                CurrentDrawingContext.PopBitmapBlendMode();
                
                renderTargetSave.Dispose();
            }
        }


        public void DrawBitmap(Bitmap bitmap, Rect src, Rect dst, Paint paint)
        {
            CurrentDrawingContext.DrawBitmap(bitmap.PlatformImpl, paint.Alpha, src, dst);
        }

        public void Clear(Color color)
        {
            CurrentDrawingContext.Clear(color);

            _matrixSaves.Clear();
            _flagSaves.Clear();
            _clipSaves.Clear();
        }

        public void Translate(double dx, double dy)
        {
            _matrix = MatrixExt.PreTranslate(_matrix, dx, dy);
        }

        public void SetMatrix(Matrix matrix)
        {
            _matrix = matrix;
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

            CurrentDrawingContext.DrawText(finalBrush, new Point(0, 0), textLayout.PlatformImpl);
            return new Rect(0, 0, textLayout.Bounds.Width, textLayout.Bounds.Height);
        }

        private class RenderTargetHolder
        {
            public IDrawingContextImpl DrawingContext { get; set; }
            public RenderTargetBitmap Bitmap { get; set; }
            public double SessionHeight { get; set; }
            public double SessionWidth { get; set; }
        }

        private class ClipSave
        {
            public ClipSave(Rect rect, IDisposable layer)
            {
                Rect = rect;
                Layer = layer;
            }

            public Rect Rect { get; }
            public IDisposable Layer { get; }
        }

        private struct RenderTargetSave
        {
            public RenderTargetSave(IDrawingContextLayerImpl layer, IDrawingContextImpl layerCtx, Size bitmapSize,
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
            public IDrawingContextImpl Context { get; }

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