﻿using System.Diagnostics;
using System.Numerics;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Value;
using Newtonsoft.Json;

namespace Avalonia.Lottie.Parser
{
    public static class AnimatableTransformParser
    {
        public static AnimatableTransform Parse(JsonReader reader, LottieComposition composition)
        {
            AnimatablePathValue anchorPoint = null;
            IAnimatableValue<Vector?, Vector?> position = null;
            AnimatableScaleValue scale = null;
            AnimatableFloatValue rotation = null;
            AnimatableIntegerValue opacity = null;
            AnimatableFloatValue startOpacity = null;
            AnimatableFloatValue endOpacity = null;

            var isObject = reader.Peek() == JsonToken.StartObject;
            if (isObject) reader.BeginObject();
            while (reader.HasNext())
                switch (reader.NextName())
                {
                    case "a":
                        reader.BeginObject();
                        while (reader.HasNext())
                            if (reader.NextName().Equals("k"))
                                anchorPoint = AnimatablePathValueParser.Parse(reader, composition);
                            else
                                reader.SkipValue();
                        reader.EndObject();
                        break;
                    case "p":
                        position = AnimatablePathValueParser.ParseSplitPath(reader, composition);
                        break;
                    case "s":
                        scale = AnimatableValueParser.ParseScale(reader, composition);
                        break;
                    case "rz":
                        composition.AddWarning("Lottie doesn't support 3D layers.");
                        rotation = AnimatableValueParser.ParseFloat(reader, composition);
                        break;
                    case "r":
                        rotation = AnimatableValueParser.ParseFloat(reader, composition);
                        if (rotation.Keyframes.Count == 0)
                            rotation.Keyframes.Add(new Keyframe<float?>(composition, 0f, 0f, null, 0f,
                                composition.EndFrame));
                        else if (rotation.Keyframes[0].StartValue == null)
                            rotation.Keyframes[0] =
                                new Keyframe<float?>(composition, 0f, 0f, null, 0f, composition.EndFrame);
                        break;
                    case "o":
                        opacity = AnimatableValueParser.ParseInteger(reader, composition);
                        break;
                    case "so":
                        startOpacity = AnimatableValueParser.ParseFloat(reader, composition);
                        break;
                    case "eo":
                        endOpacity = AnimatableValueParser.ParseFloat(reader, composition);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }

            if (isObject) reader.EndObject();

            if (anchorPoint == null)
            {
                // Cameras don't have an anchor point property. Although we don't support them, at least 
                // we won't crash. 
                Debug.WriteLine(
                    "Layer has no transform property. You may be using an unsupported layer type such as a camera.",
                    LottieLog.Tag);
                anchorPoint = new AnimatablePathValue();
            }

            if (scale == null)
                // Somehow some community animations don't have scale in the transform. 
                scale = new AnimatableScaleValue(new ScaleXy(1f, 1f));

            if (opacity == null)
                // Repeaters have start/end opacity instead of opacity 
                opacity = new AnimatableIntegerValue();

            return new AnimatableTransform(
                anchorPoint, position, scale, rotation, opacity, startOpacity, endOpacity);
        }
    }
}