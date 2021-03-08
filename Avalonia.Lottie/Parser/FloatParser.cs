namespace Avalonia.Lottie.Parser
{
    public class FloatParser : IValueParser<float?>
    {
        public static readonly FloatParser Instance = new();

        public float? Parse(JsonReader reader, float scale)
        {
            return JsonUtils.ValueFromObject(reader) * scale;
        }
    }
}
