using System.Numerics;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Model.Content;
using Avalonia.Lottie.Model.Layer;

namespace Avalonia.Lottie.Model.Animatable
{
    public class AnimatableTransform : IModifierContent, IContentModel
    {
        public AnimatableTransform()
            : this(
                new AnimatablePathValue(),
                new AnimatablePathValue(),
                new AnimatableScaleValue(),
                new AnimatableFloatValue(),
                new AnimatableIntegerValue(),
                new AnimatableFloatValue(),
                new AnimatableFloatValue()
            )
        {
        }

        public AnimatableTransform(AnimatablePathValue anchorPoint, IAnimatableValue<Vector2?, Vector2?> position,
            AnimatableScaleValue scale, AnimatableFloatValue rotation, AnimatableIntegerValue opacity,
            AnimatableFloatValue startOpacity, AnimatableFloatValue endOpacity)
        {
            AnchorPoint = anchorPoint;
            Position = position;
            Scale = scale;
            Rotation = rotation;
            Opacity = opacity;
            StartOpacity = startOpacity;
            EndOpacity = endOpacity;
        }

        internal virtual AnimatablePathValue AnchorPoint { get; }

        internal virtual IAnimatableValue<Vector2?, Vector2?> Position { get; }

        internal virtual AnimatableScaleValue Scale { get; }

        internal virtual AnimatableFloatValue Rotation { get; }

        internal virtual AnimatableIntegerValue Opacity { get; }

        // Used for repeaters 
        internal virtual AnimatableFloatValue StartOpacity { get; }
        internal virtual AnimatableFloatValue EndOpacity { get; }

        public IContent ToContent(Lottie drawable, BaseLayer layer)
        {
            return null;
        }

        public virtual TransformKeyframeAnimation CreateAnimation()
        {
            return new(this);
        }
    }
}