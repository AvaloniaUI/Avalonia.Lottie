using System.Collections.Generic;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Value;
using Newtonsoft.Json;

namespace Avalonia.Lottie.Parser
{
    internal static class KeyframesParser
    {
        internal static List<Keyframe<T>> Parse<T>(JsonReader reader, LottieComposition composition,
            IValueParser<T> valueParser)
        {
            var keyframes = new List<Keyframe<T>>();

            if (reader.Peek() == JsonToken.String)
            {
                composition.AddWarning("Lottie doesn't support expressions.");
                return keyframes;
            }

            reader.BeginObject();
            while (reader.HasNext())
                switch (reader.NextName())
                {
                    case "k":
                        if (reader.Peek() == JsonToken.StartArray)
                        {
                            reader.BeginArray();

                            if (reader.Peek() == JsonToken.Integer || reader.Peek() == JsonToken.Float)
                                // For properties in which the static value is an array of numbers. 
                                keyframes.Add(KeyframeParser.Parse(reader, composition,   valueParser, false));
                            else
                                while (reader.HasNext())
                                    keyframes.Add(KeyframeParser.Parse(reader, composition,  valueParser, true));
                            reader.EndArray();
                        }
                        else
                        {
                            keyframes.Add(KeyframeParser.Parse(reader, composition, valueParser, false));
                        }

                        break;
                    default:
                        reader.SkipValue();
                        break;
                }

            reader.EndObject();

            SetEndFrames<Keyframe<T>, T>(keyframes);
            return keyframes;
        }

        /// <summary>
        ///     The json doesn't include end frames. The data can be taken from the start frame of the next
        ///     keyframe though.
        /// </summary>
        /// <typeparam name="TU"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="keyframes"></param>
        public static void SetEndFrames<TU, TV>(List<TU> keyframes) where TU : Keyframe<TV>
        {
            var size = keyframes.Count;
            for (var i = 0; i < size - 1; i++)
            {
                // In the json, the keyframes only contain their starting frame. 
                var keyframe = keyframes[i];
                Keyframe<TV> nextKeyframe = keyframes[i + 1];
                keyframe.EndFrame = nextKeyframe.StartFrame;
                if (keyframe.EndValue == null && nextKeyframe.StartValue != null)
                {
                    keyframe.EndValue = nextKeyframe.StartValue;
                    (keyframe as PathKeyframe)?.CreatePath();
                }
            }

            var lastKeyframe = keyframes[size - 1];
            if ((lastKeyframe.StartValue == null || lastKeyframe.EndValue == null) && keyframes.Count > 1)
                // The only purpose the last keyframe has is to provide the end frame of the previous 
                // keyframe. 
                keyframes.Remove(lastKeyframe);
        }
    }
}