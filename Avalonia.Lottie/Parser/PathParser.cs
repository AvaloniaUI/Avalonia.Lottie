using System.Numerics;


namespace Avalonia.Lottie.Parser
{
    public class PathParser : IValueParser<Vector2?>
    {
        public static readonly PathParser Instance = new();

        public Vector2? Parse(JsonReader reader, float scale)
        {
            return JsonUtils.JsonToPoint(reader, scale);
        }
    }
}
