using System.Numerics;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Model.Layer
{
    internal class SolidLayer : BaseLayer
    {
        private readonly Paint _paint = new();
        private readonly Path _path = new();
        private IBaseKeyframeAnimation<ColorFilter, ColorFilter> _colorFilterAnimation;
        private Vector[] _points = new Vector[4];

        internal SolidLayer(Lottie lottie, Layer layerModel) : base(lottie, layerModel)
        {
            LayerModel = layerModel;

            _paint.Alpha = 0;
            _paint.Style = Paint.PaintStyle.Fill;
            _paint.Color = layerModel.SolidColor;
        }

        public override void DrawLayer(BitmapCanvas canvas, Matrix parentMatrix, byte parentAlpha)
        {
            int backgroundAlpha = LayerModel.SolidColor.A;
            if (backgroundAlpha == 0) return;

            var alpha = (byte) (parentAlpha / 255f * (backgroundAlpha / 255f * Transform.Opacity.Value / 100f) * 255);
            _paint.Alpha = alpha;
            if (_colorFilterAnimation != null) _paint.ColorFilter = _colorFilterAnimation.Value;
            if (alpha > 0)
            {
                _points[0] = new Vector(0, 0);
                _points[1] = new Vector(LayerModel.SolidWidth, 0);
                _points[2] = new Vector(LayerModel.SolidWidth, LayerModel.SolidHeight);
                _points[3] = new Vector(0, LayerModel.SolidHeight);

                // We can't map Rect here because if there is rotation on the transform then we aren't 
                // actually drawing a rect. 
                parentMatrix.MapPoints(ref _points);
                _path.Reset();
                _path.MoveTo(_points[0].X, _points[0].Y);
                _path.LineTo(_points[1].X, _points[1].Y);
                _path.LineTo(_points[2].X, _points[2].Y);
                _path.LineTo(_points[3].X, _points[3].Y);
                _path.LineTo(_points[0].X, _points[0].Y);
                _path.Close();
                canvas.DrawPath(_path, _paint);
            }
        }

        public override void GetBounds(ref Rect outBounds, Matrix parentMatrix)
        {
            base.GetBounds(ref outBounds, parentMatrix);
            RectExt.Set(ref Rect, 0, 0, LayerModel.SolidWidth, LayerModel.SolidHeight);
            BoundsMatrix.MapRect(ref Rect);
            RectExt.Set(ref outBounds, Rect);
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