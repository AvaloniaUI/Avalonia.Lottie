using System.Numerics;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Layer;

namespace Avalonia.Lottie.Model.Content
{
    public class PolystarShape : IContentModel
    {
        public enum Type
        {
            Star = 1,
            Polygon = 2
        }

        private readonly Type _type;

        public PolystarShape(string name, Type type, AnimatableFloatValue points,
            IAnimatableValue<Vector2?, Vector2?> position, AnimatableFloatValue rotation,
            AnimatableFloatValue innerRadius, AnimatableFloatValue outerRadius, AnimatableFloatValue innerRoundedness,
            AnimatableFloatValue outerRoundedness)
        {
            Name = name;
            _type = type;
            Points = points;
            Position = position;
            Rotation = rotation;
            InnerRadius = innerRadius;
            OuterRadius = outerRadius;
            InnerRoundedness = innerRoundedness;
            OuterRoundedness = outerRoundedness;
        }

        internal virtual string Name { get; }

        internal virtual AnimatableFloatValue Points { get; }

        internal virtual IAnimatableValue<Vector2?, Vector2?> Position { get; }

        internal virtual AnimatableFloatValue Rotation { get; }

        internal virtual AnimatableFloatValue InnerRadius { get; }

        internal virtual AnimatableFloatValue OuterRadius { get; }

        internal virtual AnimatableFloatValue InnerRoundedness { get; }

        internal virtual AnimatableFloatValue OuterRoundedness { get; }

        public IContent ToContent(Lottie drawable, BaseLayer layer)
        {
            return new PolystarContent(drawable, layer, this);
        }

        internal new virtual Type GetType()
        {
            return _type;
        }
    }
}