using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Layer;

namespace Avalonia.Lottie.Model.Content
{
    public class GradientFill : IContentModel
    {
        public GradientFill(string name, GradientType gradientType, PathFillType fillType,
            AnimatableGradientColorValue gradientColor, AnimatableIntegerValue opacity, AnimatablePointValue startPoint,
            AnimatablePointValue endPoint, AnimatableFloatValue highlightLength, AnimatableFloatValue highlightAngle)
        {
            GradientType = gradientType;
            FillType = fillType;
            GradientColor = gradientColor;
            Opacity = opacity;
            StartPoint = startPoint;
            EndPoint = endPoint;
            Name = name;
            HighlightLength = highlightLength;
            HighlightAngle = highlightAngle;
        }

        internal virtual string Name { get; }

        internal virtual GradientType GradientType { get; }

        internal virtual PathFillType FillType { get; }

        internal virtual AnimatableGradientColorValue GradientColor { get; }

        internal virtual AnimatableIntegerValue Opacity { get; }

        internal virtual AnimatablePointValue StartPoint { get; }

        internal virtual AnimatablePointValue EndPoint { get; }

        internal virtual AnimatableFloatValue HighlightLength { get; }

        internal virtual AnimatableFloatValue HighlightAngle { get; }

        public IContent ToContent(Lottie drawable, BaseLayer layer)
        {
            return new GradientFillContent(drawable, layer, this);
        }
    }
}