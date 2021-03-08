using Avalonia.Lottie.Value;
using Newtonsoft.Json;

namespace Avalonia.Lottie.Parser
{
    public class ScaleXyParser : IValueParser<ScaleXy>
    {
        public static readonly ScaleXyParser Instance = new();

        public ScaleXy Parse(JsonReader reader, float scale)
        {
            var isArray = reader.Peek() == JsonToken.StartArray;
            if (isArray)
            {
                reader.BeginArray();
            }
            var sx = reader.NextDouble();
            var sy = reader.NextDouble();
            while (reader.HasNext())
            {
                reader.SkipValue();
            }
            if (isArray)
            {
                reader.EndArray();
            }
            return new ScaleXy(sx / 100f * scale, sy / 100f * scale);
        }
    }
}
