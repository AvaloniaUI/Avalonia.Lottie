using System.Numerics;
using LottieSharp.Animation.Keyframe;
using LottieSharp.Value;
using Newtonsoft.Json;


namespace LottieSharp.Parser
{
    static class PathKeyframeParser
    {
        internal static PathKeyframe Parse(JsonReader reader, LottieComposition composition)
        {
            var animated = reader.Peek() == JsonToken.StartObject;
            var keyframe = KeyframeParser.Parse(reader, composition, Utils.Utils.DpScale(), PathParser.Instance, animated);

            return new PathKeyframe(composition, keyframe);
        }
    }
}
