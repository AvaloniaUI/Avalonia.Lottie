using System;

namespace Avalonia.Lottie.Parser
{
    public class IntegerParser : IValueParser<int?>
    {
        public static readonly IntegerParser Instance = new();

        public int? Parse(JsonReader reader)
        {
            return (int) Math.Round(JsonUtils.ValueFromObject(reader));
        }
    }
}