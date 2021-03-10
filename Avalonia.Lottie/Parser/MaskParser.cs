﻿using System.Diagnostics;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Content;

namespace Avalonia.Lottie.Parser
{
    internal static class MaskParser
    {
        internal static Mask Parse(JsonReader reader, LottieComposition composition)
        {
            var maskMode = Mask.MaskMode.MaskModeAdd;
            AnimatableShapeValue maskPath = null;
            AnimatableIntegerValue opacity = null;

            reader.BeginObject();
            while (reader.HasNext())
            {
                var mode = reader.NextName();
                switch (mode)
                {
                    case "mode":
                        switch (reader.NextString())
                        {
                            case "a":
                                maskMode = Mask.MaskMode.MaskModeAdd;
                                break;
                            case "s":
                                maskMode = Mask.MaskMode.MaskModeSubtract;
                                break;
                            case "i":
                                composition.AddWarning(
                                    "Animation contains intersect masks. They are not supported but will be treated like add masks.");
                                maskMode = Mask.MaskMode.MaskModeIntersect;
                                break;
                            default:
                                Debug.WriteLine($"Unknown mask mode {mode}. Defaulting to Add.", LottieLog.Tag);
                                maskMode = Mask.MaskMode.MaskModeAdd;
                                break;
                        }

                        break;
                    case "pt":
                        maskPath = AnimatableValueParser.ParseShapeData(reader, composition);
                        break;
                    case "o":
                        opacity = AnimatableValueParser.ParseInteger(reader, composition);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            reader.EndObject();

            return new Mask(maskMode, maskPath, opacity);
        }
    }
}