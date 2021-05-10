using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Layer;

namespace Avalonia.Lottie.Model.Content
{
    public class ShapePath : IContentModel
    {
        private readonly int _index;
        private readonly string _name;
        private readonly AnimatableShapeValue _shapePath;

        public ShapePath(string name, int index, AnimatableShapeValue shapePath)
        {
            _name = name;
            _index = index;
            _shapePath = shapePath;
        }

        public virtual string Name => _name;

        public IContent ToContent(Lottie drawable, BaseLayer layer)
        {
            return new ShapeContent(drawable, layer, this);
        }

        internal virtual AnimatableShapeValue GetShapePath()
        {
            return _shapePath;
        }

        public override string ToString()
        {
            return "ShapePath{" + "name=" + _name + ", index=" + _index + '}';
        }
    }
}