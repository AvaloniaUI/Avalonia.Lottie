using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Model;
using Avalonia.Lottie.Model.Content;
using Avalonia.Lottie.Model.Layer;
using Avalonia.Lottie.Value;
using Avalonia.Media;

namespace Avalonia.Lottie.Animation.Content
{
    internal class StrokeContent : BaseStrokeContent
    {
        private readonly IBaseKeyframeAnimation<Color?, Color?> _colorAnimation;
        private readonly BaseLayer _layer;
        private IBaseKeyframeAnimation<ColorFilter, ColorFilter> _colorFilterAnimation;

        internal StrokeContent(Lottie lottie, BaseLayer layer, ShapeStroke stroke)
            : base(lottie, layer, ShapeStroke.LineCapTypeToPaintCap(stroke.CapType),
                ShapeStroke.LineJoinTypeToPaintLineJoin(stroke.JoinType), stroke.MiterLimit, stroke.Opacity,
                stroke.Width, stroke.LineDashPattern, stroke.DashOffset)
        {
            _layer = layer;
            Name = stroke.Name;
            _colorAnimation = stroke.Color.CreateAnimation();
            _colorAnimation.ValueChanged += OnValueChanged;
            layer.AddAnimation(_colorAnimation);
        }

        public override string Name { get; }

        public override void Draw(LottieCanvas canvas, Matrix parentMatrix, byte parentAlpha)
        {
            Paint.Color = _colorAnimation.Value ?? Colors.White;
            if (_colorFilterAnimation != null) Paint.ColorFilter = _colorFilterAnimation.Value;
            base.Draw(canvas, parentMatrix, parentAlpha);
        }

        public override void AddValueCallback<T>(LottieProperty property, ILottieValueCallback<T> callback)
        {
            base.AddValueCallback(property, callback);
            if (property == LottieProperty.StrokeColor)
            {
                _colorAnimation.SetValueCallback((ILottieValueCallback<Color?>) callback);
            }
            else if (property == LottieProperty.ColorFilter)
            {
                if (callback == null)
                {
                    _colorFilterAnimation = null;
                }
                else
                {
                    _colorFilterAnimation =
                        new ValueCallbackKeyframeAnimation<ColorFilter, ColorFilter>(
                            (ILottieValueCallback<ColorFilter>) callback);
                    _colorFilterAnimation.ValueChanged += OnValueChanged;
                    _layer.AddAnimation(_colorAnimation);
                }
            }
        }
    }
}