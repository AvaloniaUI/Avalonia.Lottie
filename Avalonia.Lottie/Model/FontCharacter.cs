using System.Collections.Generic;
using Avalonia.Lottie.Model.Content;

namespace Avalonia.Lottie.Model
{
    public class FontCharacter
    {
        private readonly char _character;
        private readonly string _fontFamily;

        private readonly string _style;

        private double _size;

        public FontCharacter(List<ShapeGroup> shapes, char character, double size, double width, string style,
            string fontFamily)
        {
            Shapes = shapes;
            _character = character;
            _size = size;
            Width = width;
            _style = style;
            _fontFamily = fontFamily;
        }

        public List<ShapeGroup> Shapes { get; }

        public double Width { get; }

        internal static int HashFor(char character, string fontFamily, string style)
        {
            var result = 0;
            result = 31 * result + character;
            result = 31 * result + fontFamily.GetHashCode();
            result = 31 * result + style.GetHashCode();
            return result;
        }

        public override int GetHashCode()
        {
            return HashFor(_character, _fontFamily, _style);
        }
    }
}