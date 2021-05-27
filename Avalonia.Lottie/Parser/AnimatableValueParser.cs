using System.Collections.Generic;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Parser
{
    public static class AnimatableValueParser
    {
        public static AnimatableFloatValue ParseFloat(JsonReader reader, LottieComposition composition)
        {
            return new(Parse(reader, composition, FloatParser.Instance));
        }
 
        internal static AnimatableIntegerValue ParseInteger(JsonReader reader, LottieComposition composition)
        {
            return new(Parse(reader, composition, IntegerParser.Instance));
        }

        internal static AnimatablePointValue ParsePoint(JsonReader reader, LottieComposition composition)
        {
            return new(Parse(reader, composition, PointFParser.Instance));
        }

        internal static AnimatableScaleValue ParseScale(JsonReader reader, LottieComposition composition)
        {
            return new(Parse(reader, composition, ScaleXyParser.Instance));
        }

        internal static AnimatableShapeValue ParseShapeData(JsonReader reader, LottieComposition composition)
        {
            return new(Parse(reader, composition, ShapeDataParser.Instance));
        }

        internal static AnimatableTextFrame ParseDocumentData(JsonReader reader, LottieComposition composition)
        {
            return new(Parse(reader, composition, DocumentDataParser.Instance));
        }

        internal static AnimatableColorValue ParseColor(JsonReader reader, LottieComposition composition)
        {
            return new(Parse(reader, composition, ColorParser.Instance));
        }

        internal static AnimatableGradientColorValue ParseGradientColor(JsonReader reader,
            LottieComposition composition, int points)
        {
            return new(Parse(reader, composition, new GradientColorParser(points)));
        }
 
        /// <summary>
        ///     Will return null if the animation can't be played such as if it has expressions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="scale"></param>
        /// <param name="composition"></param>
        /// <param name="valueParser"></param>
        /// <returns></returns>
        private static List<Keyframe<T>> Parse<T>(JsonReader reader, LottieComposition composition, IValueParser<T> valueParser)
        {
            return KeyframesParser.Parse(reader, composition, valueParser);
        }
    }
}