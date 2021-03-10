﻿using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Content;

namespace Avalonia.Lottie.Parser
{
    internal static class ShapePathParser
    {
        internal static ShapePath Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            var ind = 0;
            AnimatableShapeValue shape = null;

            while (reader.HasNext())
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "ind":
                        ind = reader.NextInt();
                        break;
                    case "ks":
                        shape = AnimatableValueParser.ParseShapeData(reader, composition);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }

            return new ShapePath(name, ind, shape);
        }
    }
}