using System.Numerics;

namespace Avalonia.Lottie.Parser
{
    public class PathParser : IValueParser<Vector?>
    {
        public static readonly PathParser Instance = new();

        public Vector? Parse(JsonReader reader)
        {
            return JsonUtils.JsonToPoint(reader);
        }
    }
}