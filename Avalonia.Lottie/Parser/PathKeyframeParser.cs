using Avalonia.Lottie.Animation.Keyframe;
using Newtonsoft.Json;

namespace Avalonia.Lottie.Parser
{
    internal static class PathKeyframeParser
    {
        internal static PathKeyframe Parse(JsonReader reader, LottieComposition composition)
        {
            var animated = reader.Peek() == JsonToken.StartObject;
            var keyframe =
                KeyframeParser.Parse(reader, composition, PathParser.Instance, animated);

            return new PathKeyframe(composition, keyframe);
        }
    }
}