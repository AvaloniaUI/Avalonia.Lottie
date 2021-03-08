using System.Collections.Generic;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Layer;
using Avalonia.Media;

namespace Avalonia.Lottie.Model.Content
{
    public class ShapeStroke : IContentModel
    {
        public enum LineCapType
        {
            Butt,
            Round,
            Unknown
        }

        internal static PenLineCap LineCapTypeToPaintCap(LineCapType lineCapType)
        {
            switch (lineCapType)
            {
                case LineCapType.Butt:
                    return PenLineCap.Flat;
                case LineCapType.Round:
                    return PenLineCap.Round;
                case LineCapType.Unknown:
                default:
                    return PenLineCap.Square;
            }
        }

        public enum LineJoinType
        {
            Miter,
            Round,
            Bevel
        }

        internal static PenLineJoin LineJoinTypeToPaintLineJoin(LineJoinType lineJoinType)
        {
            switch (lineJoinType)
            {
                case LineJoinType.Bevel:
                    return PenLineJoin.Bevel;
                case LineJoinType.Miter:
                    return PenLineJoin.Miter;
                case LineJoinType.Round:
                default:
                    return PenLineJoin.Round;
            }
        }

        public ShapeStroke(string name, AnimatableFloatValue offset, List<AnimatableFloatValue> lineDashPattern, AnimatableColorValue color, AnimatableIntegerValue opacity, AnimatableFloatValue width, LineCapType capType, LineJoinType joinType, float miterLimit)
        {
            Name = name;
            DashOffset = offset;
            LineDashPattern = lineDashPattern;
            Color = color;
            Opacity = opacity;
            Width = width;
            CapType = capType;
            JoinType = joinType;
            MiterLimit = miterLimit;
        }

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new StrokeContent(drawable, layer, this);
        }

        internal virtual string Name { get; }

        internal virtual AnimatableColorValue Color { get; }

        internal virtual AnimatableIntegerValue Opacity { get; }

        internal virtual AnimatableFloatValue Width { get; }

        internal virtual List<AnimatableFloatValue> LineDashPattern { get; }

        internal virtual AnimatableFloatValue DashOffset { get; }

        internal virtual LineCapType CapType { get; }

        internal virtual LineJoinType JoinType { get; }

        internal virtual float MiterLimit { get; }
    }
}