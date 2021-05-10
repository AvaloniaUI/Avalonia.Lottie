using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Layer;

namespace Avalonia.Lottie.Model.Content
{
    public class ShapeTrimPath : IContentModel
    {
        public enum Type
        {
            Simultaneously = 1,
            Individually = 2
        }

        private readonly AnimatableFloatValue _end;
        private readonly AnimatableFloatValue _offset;
        private readonly AnimatableFloatValue _start;

        private readonly Type _type;

        public ShapeTrimPath(string name, Type type, AnimatableFloatValue start, AnimatableFloatValue end,
            AnimatableFloatValue offset)
        {
            Name = name;
            _type = type;
            _start = start;
            _end = end;
            _offset = offset;
        }

        internal virtual string Name { get; }

        internal virtual AnimatableFloatValue End => _end;

        internal virtual AnimatableFloatValue Start => _start;

        internal virtual AnimatableFloatValue Offset => _offset;

        public IContent ToContent(Lottie drawable, BaseLayer layer)
        {
            return new TrimPathContent(layer, this);
        }

        internal new virtual Type GetType()
        {
            return _type;
        }

        public override string ToString()
        {
            return "Trim Path: {start: " + _start + ", end: " + _end + ", offset: " + _offset + "}";
        }
    }
}