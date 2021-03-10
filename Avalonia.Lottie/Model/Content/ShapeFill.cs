using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Layer;

namespace Avalonia.Lottie.Model.Content
{
    public class ShapeFill : IContentModel
    {
        private readonly bool _fillEnabled;

        public ShapeFill(string name, bool fillEnabled, PathFillType fillType, AnimatableColorValue color,
            AnimatableIntegerValue opacity)
        {
            Name = name;
            _fillEnabled = fillEnabled;
            FillType = fillType;
            Color = color;
            Opacity = opacity;
        }

        internal virtual string Name { get; }

        internal virtual AnimatableColorValue Color { get; }

        internal virtual AnimatableIntegerValue Opacity { get; }

        internal virtual PathFillType FillType { get; }

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new FillContent(drawable, layer, this);
        }

        public override string ToString()
        {
            return "ShapeFill{" + "color=" + ", fillEnabled=" + _fillEnabled + '}';
        }
    }
}