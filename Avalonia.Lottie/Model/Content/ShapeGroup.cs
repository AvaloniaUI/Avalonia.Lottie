using System.Collections.Generic;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Model.Layer;

namespace Avalonia.Lottie.Model.Content
{
    public class ShapeGroup : IContentModel
    {
        private readonly List<IContentModel> _items;
        private readonly string _name;

        public ShapeGroup(string name, List<IContentModel> items)
        {
            _name = name;
            _items = items;
        }

        public virtual string Name => _name;

        public virtual List<IContentModel> Items => _items;

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new ContentGroup(drawable, layer, this);
        }

        public override string ToString()
        {
            return "ShapeGroup{" + "name='" + _name + "\' Shapes: " + "[" + string.Join(",", _items) + "]" + "}";
        }
    }
}