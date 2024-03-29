﻿using System.Numerics;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Content;

namespace Avalonia.Lottie.Parser
{
    internal static class RectangleShapeParser
    {
        internal static RectangleShape Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            IAnimatableValue<Vector?, Vector?> position = null;
            AnimatablePointValue size = null;
            AnimatableFloatValue roundedness = null;

            while (reader.HasNext())
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "p":
                        position = AnimatablePathValueParser.ParseSplitPath(reader, composition);
                        break;
                    case "s":
                        size = AnimatableValueParser.ParsePoint(reader, composition);
                        break;
                    case "r":
                        roundedness = AnimatableValueParser.ParseFloat(reader, composition);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }

            return new RectangleShape(name, position, size, roundedness);
        }
    }
}