﻿using System.Numerics;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Content;

namespace Avalonia.Lottie.Parser
{
    internal static class PolystarShapeParser
    {
        internal static PolystarShape Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            var type = PolystarShape.Type.Polygon;
            AnimatableFloatValue points = null;
            IAnimatableValue<Vector?, Vector?> position = null;
            AnimatableFloatValue rotation = null;
            AnimatableFloatValue outerRadius = null;
            AnimatableFloatValue outerRoundedness = null;
            AnimatableFloatValue innerRadius = null;
            AnimatableFloatValue innerRoundedness = null;

            while (reader.HasNext())
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "sy":
                        type = (PolystarShape.Type) reader.NextInt();
                        break;
                    case "pt":
                        points = AnimatableValueParser.ParseFloat(reader, composition);
                        break;
                    case "p":
                        position = AnimatablePathValueParser.ParseSplitPath(reader, composition);
                        break;
                    case "r":
                        rotation = AnimatableValueParser.ParseFloat(reader, composition);
                        break;
                    case "or":
                        outerRadius = AnimatableValueParser.ParseFloat(reader, composition);
                        break;
                    case "os":
                        outerRoundedness = AnimatableValueParser.ParseFloat(reader, composition);
                        break;
                    case "ir":
                        innerRadius = AnimatableValueParser.ParseFloat(reader, composition);
                        break;
                    case "is":
                        innerRoundedness = AnimatableValueParser.ParseFloat(reader, composition);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }

            return new PolystarShape(name, type, points, position, rotation, innerRadius, outerRadius, innerRoundedness,
                outerRoundedness);
        }
    }
}