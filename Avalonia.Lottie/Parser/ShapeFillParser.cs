﻿using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Content;

namespace Avalonia.Lottie.Parser
{
    internal static class ShapeFillParser
    {
        internal static ShapeFill Parse(JsonReader reader, LottieComposition composition)
        {
            AnimatableColorValue color = null;
            var fillEnabled = false;
            AnimatableIntegerValue opacity = null;
            string name = null;
            var fillTypeInt = 1;

            while (reader.HasNext())
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "c":
                        color = AnimatableValueParser.ParseColor(reader, composition);
                        break;
                    case "o":
                        opacity = AnimatableValueParser.ParseInteger(reader, composition);
                        break;
                    case "fillEnabled":
                        fillEnabled = reader.NextBoolean();
                        break;
                    case "r":
                        fillTypeInt = reader.NextInt();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }

            var fillType = fillTypeInt == 1 ? PathFillType.Winding : PathFillType.EvenOdd;
            return new ShapeFill(name, fillEnabled, fillType, color, opacity);
        }
    }
}