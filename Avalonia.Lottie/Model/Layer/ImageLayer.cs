using System;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Value;
using Avalonia.Media.Imaging;

namespace Avalonia.Lottie.Model.Layer
{
    internal class ImageLayer : BaseLayer
    {
        private readonly Paint _paint = new(Paint.AntiAliasFlag | Paint.FilterBitmapFlag);
        private IBaseKeyframeAnimation<ColorFilter, ColorFilter> _colorFilterAnimation;
        private Rect _dst;
        private Rect _src;

        internal ImageLayer(Lottie lottie, Layer layerModel) : base(lottie, layerModel)
        {
        }

        private int PixelWidth => Bitmap.PixelSize.Width;

        private int PixelHeight => Bitmap.PixelSize.Height;

        private Bitmap Bitmap
        {
            get
            {
                var refId = LayerModel.RefId;
                return Lottie.GetImageAsset(refId);
            }
        }

        public override void DrawLayer(BitmapCanvas canvas, Matrix3X3 parentMatrix, byte parentAlpha)
        {
            var bitmap = Bitmap;
            if (bitmap == null) return;
            var density = Utils.Utils.DpScale();

            _paint.Alpha = parentAlpha;
            if (_colorFilterAnimation != null) _paint.ColorFilter = _colorFilterAnimation.Value;
            canvas.Save();
            canvas.Concat(parentMatrix);
            RectExt.Set(ref _src, 0, 0, PixelWidth, PixelHeight);
            RectExt.Set(ref _dst, 0, 0, (int) (PixelWidth * density), (int) (PixelHeight * density));
            canvas.DrawBitmap(bitmap, _src, _dst, _paint);
            canvas.Restore();
        }

        public override void GetBounds(ref Rect outBounds, Matrix3X3 parentMatrix)
        {
            base.GetBounds(ref outBounds, parentMatrix);
            var bitmap = Bitmap;
            if (bitmap != null)
            {
                RectExt.Set(ref outBounds,  outBounds.Left,  outBounds.Top,
                     Math.Min(outBounds.Right, PixelWidth),  Math.Min(outBounds.Bottom, PixelHeight));
                BoundsMatrix.MapRect(ref outBounds);
            }
        }

        public override void AddValueCallback<T>(LottieProperty property, ILottieValueCallback<T> callback)
        {
            base.AddValueCallback(property, callback);
            if (property == LottieProperty.ColorFilter)
            {
                if (callback == null)
                    _colorFilterAnimation = null;
                else
                    _colorFilterAnimation =
                        new ValueCallbackKeyframeAnimation<ColorFilter, ColorFilter>(
                            (ILottieValueCallback<ColorFilter>) callback);
            }
        }
    }
}