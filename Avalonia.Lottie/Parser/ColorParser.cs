using Avalonia.Media;
using Newtonsoft.Json;

namespace Avalonia.Lottie.Parser
{
    internal class ColorParser : IValueParser<Color?>
    {
        internal static readonly ColorParser Instance = new();

        public Color? Parse(JsonReader reader, double  scale)
        {
            var isArray = reader.Peek() == JsonToken.StartArray;
            if (isArray) reader.BeginArray();
            var r = reader.NextDouble();
            var g = reader.NextDouble();
            var b = reader.NextDouble();
            var a = reader.NextDouble();
            if (isArray) reader.EndArray();

            if (r <= 1 && g <= 1 && b <= 1 && a <= 1)
            {
                r *= 255;
                g *= 255;
                b *= 255;
                a *= 255;
            }

            return new Color((byte) a, (byte) r, (byte) g, (byte) b);
        }
    }
}