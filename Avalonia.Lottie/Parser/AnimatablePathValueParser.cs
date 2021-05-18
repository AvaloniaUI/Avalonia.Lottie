using System.Collections.Generic;
using System.Numerics;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Value;
using Newtonsoft.Json;

namespace Avalonia.Lottie.Parser
{
    public static class AnimatablePathValueParser
    {
        public static AnimatablePathValue Parse(JsonReader reader, LottieComposition composition)
        {
            var keyframes = new List<Keyframe<Vector?>>();
            if (reader.Peek() == JsonToken.StartArray)
            {
                reader.BeginArray();
                while (reader.HasNext()) keyframes.Add(PathKeyframeParser.Parse(reader, composition));
                reader.EndArray();
                KeyframesParser.SetEndFrames<Keyframe<Vector?>, Vector?>(keyframes);
            }
            else
            {
                keyframes.Add(new Keyframe<Vector?>(JsonUtils.JsonToPoint(reader, Utils.Utils.DpScale())));
            }

            return new AnimatablePathValue(keyframes);
        }

        /// <summary>
        ///     Returns either an <see cref="AnimatablePathValue" /> or an <see cref="AnimatableSplitDimensionPathValue" />.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="composition"></param>
        /// <returns></returns>
        internal static IAnimatableValue<Vector?, Vector?> ParseSplitPath(JsonReader reader,
            LottieComposition composition)
        {
            AnimatablePathValue pathAnimation = null;
            AnimatableFloatValue xAnimation = null;
            AnimatableFloatValue yAnimation = null;

            var hasExpressions = false;

            reader.BeginObject();
            while (reader.Peek() != JsonToken.EndObject)
                switch (reader.NextName())
                {
                    case "k":
                        pathAnimation = Parse(reader, composition);
                        break;
                    case "x":
                        if (reader.Peek() == JsonToken.String)
                        {
                            hasExpressions = true;
                            reader.SkipValue();
                        }
                        else
                        {
                            xAnimation = AnimatableValueParser.ParseFloat(reader, composition);
                        }

                        break;
                    case "y":
                        if (reader.Peek() == JsonToken.String)
                        {
                            hasExpressions = true;
                            reader.SkipValue();
                        }
                        else
                        {
                            yAnimation = AnimatableValueParser.ParseFloat(reader, composition);
                        }

                        break;
                    default:
                        reader.SkipValue();
                        break;
                }

            reader.EndObject();

            if (hasExpressions) composition.AddWarning("Lottie doesn't support expressions.");

            if (pathAnimation != null) return pathAnimation;
            return new AnimatableSplitDimensionPathValue(xAnimation, yAnimation);
        }
    }
}