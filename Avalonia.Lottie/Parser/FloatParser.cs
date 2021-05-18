namespace Avalonia.Lottie.Parser
{
    public class FloatParser : IValueParser<float?>
    {
        public static readonly FloatParser Instance = new();

        public float? Parse(JsonReader reader, double  scale)
        {
            return (float?)JsonUtils.ValueFromObject(reader) * (float?)scale;
        }
    }
}