﻿using LottieSharp.Model.Animatable;
using LottieSharp.Value;
using System.Collections.Generic;

namespace LottieSharp.Parser
{
    public static class AnimatableValueParser
    {
        public static AnimatableFloatValue ParseFloat(JsonReader reader, LottieComposition composition)
        {
            return ParseFloat(reader, composition, true);
        }

        public static AnimatableFloatValue ParseFloat(JsonReader reader, LottieComposition composition, bool isDp)
        {
            return new(Parse(reader, isDp ? Utils.Utils.DpScale() : 1f, composition, FloatParser.Instance));
        }

        internal static AnimatableIntegerValue ParseInteger(JsonReader reader, LottieComposition composition)
        {
            return new(Parse(reader, composition, IntegerParser.Instance));
        }

        internal static AnimatablePointValue ParsePoint(JsonReader reader, LottieComposition composition)
        {
            return new(Parse(reader, Utils.Utils.DpScale(), composition, PointFParser.Instance));
        }

        internal static AnimatableScaleValue ParseScale(JsonReader reader, LottieComposition composition)
        {
            return new(Parse(reader, composition, ScaleXyParser.Instance));
        }

        internal static AnimatableShapeValue ParseShapeData(JsonReader reader, LottieComposition composition)
        {
            return new(Parse(reader, Utils.Utils.DpScale(), composition, ShapeDataParser.Instance));
        }

        internal static AnimatableTextFrame ParseDocumentData(JsonReader reader, LottieComposition composition)
        {
            return new(Parse(reader, composition, DocumentDataParser.Instance));
        }

        internal static AnimatableColorValue ParseColor(JsonReader reader, LottieComposition composition)
        {
            return new(Parse(reader, composition, ColorParser.Instance));
        }

        internal static AnimatableGradientColorValue ParseGradientColor(JsonReader reader, LottieComposition composition, int points)
        {
            return new(Parse(reader, composition, new GradientColorParser(points)));
        }

        /// <summary>
        /// Will return null if the animation can't be played such as if it has expressions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="composition"></param>
        /// <param name="valueParser"></param>
        /// <returns></returns>
        private static List<Keyframe<T>> Parse<T>(JsonReader reader, LottieComposition composition, IValueParser<T> valueParser)
        {
            return KeyframesParser.Parse(reader, composition, 1, valueParser);
        }

        /// <summary>
        /// Will return null if the animation can't be played such as if it has expressions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="scale"></param>
        /// <param name="composition"></param>
        /// <param name="valueParser"></param>
        /// <returns></returns>
        private static List<Keyframe<T>> Parse<T>(JsonReader reader, float scale, LottieComposition composition, IValueParser<T> valueParser)
        {
            return KeyframesParser.Parse(reader, composition, scale, valueParser);
        }
    }
}