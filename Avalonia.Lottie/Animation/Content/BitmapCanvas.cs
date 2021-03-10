using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Skia;
using Avalonia.Utilities;
using Avalonia.Visuals.Media.Imaging;
using SkiaSharp;

namespace Avalonia.Lottie.Animation.Content
{
    public class BitmapCanvas : IDisposable
    {
        public static int MatrixSaveFlag = 0b00001;

        public static int ClipSaveFlag = 0b00010;

        //public static int HasAlphaLayerSaveFlag = 0b00100;
        //public static int FullColorLayerSaveFlag = 0b01000;
        public static int ClipToLayerSaveFlag = 0b10000;
        public static int AllSaveFlag = 0b11111;
        private readonly Stack<RenderTargetHolder> _canvasDrawingSessions = new();

        private readonly Stack<ClipSave> _clipSaves = new();
        private readonly Stack<int> _flagSaves = new();
        private readonly Stack<Matrix3X3> _matrixSaves = new();

        private readonly Dictionary<int, RenderTargetHolder> _renderTargets = new();

        private readonly Stack<RenderTargetSave> _renderTargetSaves = new();
        private Rect _currentClip;
        private Matrix3X3 _matrix = Matrix3X3.CreateIdentity();

        public BitmapCanvas(double width, double height)
        {
            //OutputRenderTarget = renderTarget;
            UpdateClip(width, height);
        }

        //internal RenderTarget OutputRenderTarget { get; private set; }
        internal IDrawingContextImpl CurrentDrawingContext =>
            _canvasDrawingSessions.Count > 0 ? _canvasDrawingSessions.Peek()?.DrawingContext : null;

        public double Width { get; private set; }
        public double Height { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void UpdateClip(double width, double height)
        {
            if (Math.Abs(width - Width) > 0 || Math.Abs(height - Height) > 0) Dispose(false);

            Width = width;
            Height = height;
            _currentClip = new Rect(0, 0, Width, Height);
        }

        internal IDisposable CreateSession(double width, double height, IDrawingContextImpl drawingSession)
        {
            _canvasDrawingSessions.Clear();
            //_renderTarget = drawingSession;
            _canvasDrawingSessions.Push(new RenderTargetHolder
            {
                SessionWidth = width,
                SessionHeight = height,
                DrawingContext = drawingSession
            });

            UpdateClip(width, height);

            return PushMask(_currentClip, 1f);
            //return new Disposable(() => { });
        }

        public void DrawRect(double x1, double y1, double x2, double y2, Paint paint)
        {
            DrawRect(new Rect(x1, y1, x2 - x1, y2 - y1), paint);
        }


        internal void DrawRect(Rect rect, Paint paint)
        {
            // UpdateDrawingSessionWithFlags(paint.Flags);

            // CurrentDrawingContext.Transform = GetCurrentTransform();
            var brush = new ImmutableSolidColorBrush(paint.Color);
            {
                if (paint.Style == Paint.PaintStyle.Stroke)
                    CurrentDrawingContext.DrawRectangle(null, new Pen(brush,
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
            // UpdateDrawingSessionWithFlags(paint.Flags);

            // CurrentDrawingContext.Transform = GetCurrentTransform();

            var gradient = paint.Shader as Gradient;
            var brush = gradient != null ? gradient.GetBrush(paint.Alpha) : new SolidColorBrush(paint.Color);
            var finalBrush = paint.ColorFilter?.Apply(this, brush) ?? brush;

            var geometry = path.GetGeometry();

            if (paint.Style == Paint.PaintStyle.Stroke)
            {
                var pen = new Pen(brush,
                    paint.StrokeWidth,
                    lineCap: paint.StrokeCap,
                    lineJoin: paint.StrokeJoin,
                    miterLimit: paint.StrokeMiter);
                CurrentDrawingContext.DrawGeometry(null, pen, geometry.PlatformImpl);
            }

            else
            {
                CurrentDrawingContext.DrawGeometry(finalBrush, null, geometry.PlatformImpl);
            }


            //     CurrentDrawingContext.DrawGeometry(geometry, finalBrush, paint.StrokeWidth, GetStrokeStyle(paint));
            // else
            //     CurrentDrawingContext.FillGeometry(geometry, finalBrush);


            // if (gradient == null)
            // {
            //     brush?.Dispose();
            //     finalBrush?.Dispose();
            // }
        }

        public Disposable PushMask(Rect rect, double alpha, Path path = null)
        {
            if (alpha >= 1 && path == null)
            {
                CurrentDrawingContext.PushClip(rect);
                // CurrentDrawingContext.PushAxisAlignedClip(rect, CurrentDrawingContext.AntialiasMode);

                return new Disposable(() => { CurrentDrawingContext.PopClip(); });
            }

            var geometery = path.GetGeometry();
            //
            // var parameters = new LayerParameters
            // {
            //     ContentBounds = rect,
            //     Opacity = alpha,
            //     MaskTransform = GetCurrentTransform(),
            //     GeometricMask = geometery
            // };
            //
            // var layer = new Layer(CurrentDrawingContext);

            CurrentDrawingContext.PushGeometryClip(geometery.PlatformImpl);

            return new Disposable(() =>
            {
                CurrentDrawingContext.PopGeometryClip();
                // this.CurrentDrawingContext.PopLayer();
                // layer.Dispose();
                // geometery?.Dispose();
            });
        }

        private Matrix GetCurrentTransform()
        {
            return new(_matrix.M11,
                    _matrix.M21,
                    _matrix.M12,
                    _matrix.M22,
                    _matrix.M13,
                    _matrix.M23)
                ;
        }

        public bool ClipRect(Rect rect)
        {
            _currentClip.Intersects(rect);
            return _currentClip.IsEmpty == false;
        }

        public void ClipReplaceRect(Rect rect)
        {
            _currentClip = rect;
        }

        public void Concat(Matrix3X3 parentMatrix)
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
                ///  UpdateDrawingSessionWithFlags(paint.Flags);

                var rendertarget = CreateRenderTarget(bounds, _renderTargetSaves.Count);
                _renderTargetSaves.Push(new RenderTargetSave(rendertarget.DrawingContext, paint.Flags, paint.Xfermode,
                    paint.Xfermode != null ? (byte) 255 : paint.Alpha));

                //var drawingSession = rendertarget.CreateDrawingSession();
                rendertarget.DrawingContext.Clear(Colors.Transparent);
                _canvasDrawingSessions.Push(rendertarget);
            }

            if ((flags & ClipSaveFlag) == ClipSaveFlag) SaveClip(isClipToLayer ? (byte) 255 : paint.Alpha, path);
        }

        private RenderTargetHolder CreateRenderTarget(Rect bounds, int index)
        {
            if (!_renderTargets.TryGetValue(index, out var rendertarget))
            {
                var bitmap = new RenderTargetBitmap(new PixelSize((int) bounds.Width, (int) bounds.Height),
                    new Vector(96, 96));

                var rt = bitmap.CreateDrawingContext(null);

                rendertarget = new RenderTargetHolder
                {
                    DrawingContext = rt,
                    Bitmap = bitmap
                };
                _renderTargets.Add(index, rendertarget);
            }

            return rendertarget;
        }

        private void SaveMatrix()
        {
            var copy = new Matrix3X3();
            copy.Set(_matrix);
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

            //if ((flags & ClipToLayerSaveFlag) == ClipToLayerSaveFlag)
            //{
            //    using (var brush = new SolidColorBrush(new RawColor4(0, 0, 0, 0.3f)))
            //        CurrentRenderTarget.FillRectangle(new Rect(0, 0, 100, 100), brush);
            //}


            if ((flags & MatrixSaveFlag) == MatrixSaveFlag) _matrix = _matrixSaves.Pop();

            if ((flags & ClipSaveFlag) == ClipSaveFlag)
            {
                var clipSave = _clipSaves.Pop();
                _currentClip = clipSave.Rect;
                clipSave.Layer.Dispose();
            }

            if ((flags & ClipToLayerSaveFlag) == ClipToLayerSaveFlag)
            {
                var drawingSession = _canvasDrawingSessions.Pop();
                // drawingSession.DrawingContext.Flush();
                // drawingSession.DrawingContext.EndDraw();

                var renderTargetSave = _renderTargetSaves.Pop();

                //  UpdateDrawingSessionWithFlags(renderTargetSave.PaintFlags);
                // CurrentDrawingContext.Transform = GetCurrentTransform();


                // var canvasComposite = CompositeMode.SourceOver;
                // if (renderTargetSave.PaintXfermode != null)
                // {
                //     canvasComposite = PorterDuff.ToCanvasComposite(renderTargetSave.PaintXfermode.Mode);
                // }
                //
                // CurrentDrawingContext.DrawBitmap(drawingSession.Bitmap.PlatformImpl,1,
                //       new Rect(0,0, drawingSession.Bitmap.Size.Width,  drawingSession.Bitmap.Size.Height) ,
                //       new Rect(0,0, drawingSession.Bitmap.Size.Width,  drawingSession.Bitmap.Size.Height) );

                var j = new Rect(0, 0, drawingSession.Bitmap.Size.Width, drawingSession.Bitmap.Size.Height);

                HackedSkiaDrawBitmap(CurrentDrawingContext, drawingSession.Bitmap.PlatformImpl, 1, j, j,
                    BitmapInterpolationMode.Default, renderTargetSave.PaintXfermode);

                // CurrentDrawingContext.DrawImage(drawingSession.Bitmap,
                //     new RawVector2(0, 0),
                //     new Rect(0, 0, renderTargetSave.RenderTarget.Size.Width, renderTargetSave.RenderTarget.Size.Height),
                //     //renderTargetSave.PaintAlpha / 255f,
                //     InterpolationMode.Linear,
                //     canvasComposite);

                //CurrentRenderTarget.DrawBitmap(drawingSession.Bitmap, 255f, InterpolationMode.Linear);


                //var rect = new RawRect(0, 0, rt.Size.Width, rt.Size.Height);
                ////using (var brush = new SolidColorBrush(Color.Black))
                ////    CurrentRenderTarget.FillOpacityMask(rt, brush, OpacityMaskContent.Graphics, rect, rect);
                //CurrentRenderTarget.DrawImage(rt, rect, renderTargetSave.PaintAlpha / 255f, BitmapInterpolationMode.Linear, rect);

                //rt.Dispose();
                //renderTargetSave.RenderTarget.Dispose();
            }

            // CurrentDrawingContext.Flush();
        }

        public static object ReflCast(object obj, Type t)
        {
            try
            {
                var param = Expression.Parameter(obj.GetType());
                return Expression.Lambda(Expression.Convert(param, t), param)
                    .Compile().DynamicInvoke(obj);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        public void HackedSkiaDrawBitmap(IDrawingContextImpl ctx, IRef<IBitmapImpl> source, double opacity,
            Rect sourceRect,
            Rect destRect, BitmapInterpolationMode bitmapInterpolationMode, PorterDuffXfermode porterDuffXfermode)
        {
            // The mother of all Hacks
            var assm = typeof(SkiaPlatform).Assembly;
            var drawableBitmapImplType = assm.GetType("Avalonia.Skia.IDrawableBitmapImpl");
            var drawingContextImplType = assm.GetType("Avalonia.Skia.DrawingContextImpl");
            var castedDrawableBitmap = ReflCast(source.Item, drawableBitmapImplType);
            var castedDrawingContextImpl = ReflCast(ctx, drawingContextImplType);
            var hackedDrawMethod = drawableBitmapImplType?.GetMethod("Draw");
            var hackedCurrentOpacityField = drawingContextImplType?.GetField("_currentOpacity", BindingFlags.NonPublic |
                BindingFlags.Instance);
            var curOpacity = (double) hackedCurrentOpacityField?.GetValue(castedDrawingContextImpl);
            var s = sourceRect.ToSKRect();
            var d = destRect.ToSKRect();

            using (var paint =
                new SKPaint
                {
                    Color = new SKColor(255, 255, 255, (byte) (255 * opacity * curOpacity))
                })
            {
                paint.FilterQuality = bitmapInterpolationMode.ToSKFilterQuality();
                if (porterDuffXfermode != null)
                    switch (porterDuffXfermode.Mode)
                    {
                        case PorterDuff.Mode.SrcAtop:
                            paint.BlendMode = SKBlendMode.SrcATop;
                            break;
                        case PorterDuff.Mode.DstOut:
                            paint.BlendMode = SKBlendMode.DstOut;
                            break;
                        case PorterDuff.Mode.DstIn:
                            paint.BlendMode = SKBlendMode.DstIn;
                            break;
                        case PorterDuff.Mode.Clear:
                            paint.BlendMode = SKBlendMode.Clear;
                            break;
                    }

                hackedDrawMethod?.Invoke(castedDrawableBitmap, new object[] {ctx, s, d, paint});
            }
        }


        public void DrawBitmap(Bitmap bitmap, Rect src, Rect dst, Paint paint)
        {
            // UpdateDrawingSessionWithFlags(paint.Flags);
            // var curMatrix = GetCurrentTransform();
            // CurrentDrawingContext.Transform = new Matrix(curMatrix.M11, curMatrix.M12, curMatrix.M21, curMatrix.M22,
            //     curMatrix.M31, curMatrix.M32);

            //var canvasComposite = CanvasComposite.SourceOver;
            // TODO paint.ColorFilter
            //if (paint.ColorFilter is PorterDuffColorFilter porterDuffColorFilter)
            //    canvasComposite = PorterDuff.ToCanvasComposite(porterDuffColorFilter.Mode);

            CurrentDrawingContext.DrawBitmap(bitmap.PlatformImpl, paint.Alpha, src, dst);
        }

        public void GetClipBounds(ref Rect bounds)
        {
            RectExt.Set(ref bounds, _currentClip.X, _currentClip.Y, _currentClip.Width, _currentClip.Height);
        }

        public void Clear(Color color)
        {
            //   UpdateDrawingSessionWithFlags(0);

            CurrentDrawingContext.Clear(color);

            _matrixSaves.Clear();
            _flagSaves.Clear();
            _clipSaves.Clear();
        }

        private void UpdateDrawingSessionWithFlags(int flags)
        {
            // CurrentDrawingContext.AntialiasMode = (flags & Paint.AntiAliasFlag) == Paint.AntiAliasFlag
            //     ? AntialiasMode.PerPrimitive
            //     : AntialiasMode.Aliased;
        }

        // private AntialiasMode GetDrawingSessionMode(int flags)
        // {
        //     return (flags & Paint.AntiAliasFlag) == Paint.AntiAliasFlag
        //         ? AntialiasMode.PerPrimitive
        //         : AntialiasMode.Aliased;
        // }

        public void Translate(double dx, double dy)
        {
            _matrix = MatrixExt.PreTranslate(_matrix, (float) dx, (float) dy);
        }

        public void Scale(double sx, double sy, double px, double py)
        {
            _matrix = MatrixExt.PreScale(_matrix, (float) sx, (float) sy, (float) px, (float) py);
        }

        public void SetMatrix(Matrix3X3 matrix)
        {
            _matrix.Set(matrix);
        }

        public Rect DrawText(char character, Paint paint)
        {
            var gradient = paint.Shader as Gradient;
            var brush = gradient != null ? gradient.GetBrush(paint.Alpha) : new SolidColorBrush(paint.Color);
            var finalBrush = paint.ColorFilter?.Apply(this, brush) ?? brush;

            //  UpdateDrawingSessionWithFlags(paint.Flags);
            // CurrentDrawingContext.Transform = GetCurrentTransform();

            var text = new string(character, 1);

            var textLayout = new FormattedText
            {
                Text = text,
                Typeface = new Media.Typeface(paint.Typeface.FontFamily, paint.Typeface.Style, paint.Typeface.Weight),
                FontSize = paint.TextSize
            };

            CurrentDrawingContext.DrawText(finalBrush, new Point(0, 0), textLayout.PlatformImpl);
            return new Rect(0, 0, textLayout.Bounds.Width, textLayout.Bounds.Height);

            // //TODO: OID: Check for global factory
            // using (var factory = new SharpDX.DirectWrite.Factory())
            // {
            //     var textFormat = new TextFormat(factory, paint.Typeface.FontFamily, paint.Typeface.Weight,
            //         paint.Typeface.Style, paint.TextSize)
            //     {
            //         //FontSize = paint.TextSize,
            //         //FontFamily = paint.Typeface.FontFamily,
            //         //FontStyle = paint.Typeface.Style,
            //         //FontWeight = paint.Typeface.Weight,
            //         //VerticalAlignment = CanvasVerticalAlignment.Center,
            //         //HorizontalAlignment = CanvasHorizontalAlignment.Left,
            //         //LineSpacingBaseline = 0,
            //         //LineSpacing = 0
            //     };
            //     var textLayout = new TextLayout(factory, text, textFormat, 0.0f, 0.0f);
            //     CurrentDrawingContext.DrawText(text, textFormat, new Rect(0, 0, 0, 0), finalBrush);
            //
            //     if (gradient == null)
            //     {
            //         brush?.Dispose();
            //         finalBrush?.Dispose();
            //     }
            //
            //     //TODO: OID: LayoutBound is not exists in text layout
            //     return new Rect(0, 0, , 0);
            // }
        }

        private void Dispose(bool disposing)
        {
            foreach (var renderTarget in _renderTargets)
            {
                renderTarget.Value.DrawingContext.Dispose();
                renderTarget.Value.DrawingContext = null;

                renderTarget.Value.Bitmap.Dispose();
                renderTarget.Value.Bitmap = null;
            }

            _renderTargets.Clear();
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

        private class RenderTargetSave
        {
            public RenderTargetSave(IDrawingContextImpl renderTarget, int paintFlags, PorterDuffXfermode paintXfermode,
                byte paintAlpha)
            {
                RenderTarget = renderTarget;
                PaintFlags = paintFlags;
                PaintXfermode = paintXfermode;
                PaintAlpha = paintAlpha;
            }

            public IDrawingContextImpl RenderTarget { get; }
            public int PaintFlags { get; }
            public PorterDuffXfermode PaintXfermode { get; }
            public byte PaintAlpha { get; }
        }
    }
}