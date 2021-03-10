using System.Numerics;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Layer;

namespace Avalonia.Lottie.Model.Content
{
    public class RectangleShape : IContentModel
    {
        private readonly IAnimatableValue<Vector2?, Vector2?> _position;
        private readonly AnimatablePointValue _size;

        public RectangleShape(string name, IAnimatableValue<Vector2?, Vector2?> position, AnimatablePointValue size,
            AnimatableFloatValue cornerRadius)
        {
            Name = name;
            _position = position;
            _size = size;
            CornerRadius = cornerRadius;
        }

        internal virtual string Name { get; }

        internal virtual AnimatableFloatValue CornerRadius { get; }

        internal virtual AnimatablePointValue Size => _size;

        internal virtual IAnimatableValue<Vector2?, Vector2?> Position => _position;

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new RectangleContent(drawable, layer, this);
        }

        public override string ToString()
        {
            return "RectangleShape{position=" + _position + ", size=" + _size + '}';
        }
    }
}