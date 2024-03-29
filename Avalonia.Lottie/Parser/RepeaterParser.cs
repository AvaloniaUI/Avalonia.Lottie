﻿using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Content;

namespace Avalonia.Lottie.Parser
{
    internal static class RepeaterParser
    {
        internal static Repeater Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            AnimatableFloatValue copies = null;
            AnimatableFloatValue offset = null;
            AnimatableTransform transform = null;

            while (reader.HasNext())
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "c":
                        copies = AnimatableValueParser.ParseFloat(reader, composition);
                        break;
                    case "o":
                        offset = AnimatableValueParser.ParseFloat(reader, composition);
                        break;
                    case "tr":
                        transform = AnimatableTransformParser.Parse(reader, composition);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }

            return new Repeater(name, copies, offset, transform);
        }
    }
}