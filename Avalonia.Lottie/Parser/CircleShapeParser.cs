using System.Numerics;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Content;


namespace Avalonia.Lottie.Parser
{
    static class CircleShapeParser
    {
        internal static CircleShape Parse(JsonReader reader, LottieComposition composition, int d)
        {
            string name = null;
            IAnimatableValue<Vector2?, Vector2?> position = null;
            AnimatablePointValue size = null;
            var reversed = d == 3;

            while (reader.HasNext())
            {
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
                    case "d":
                        // "d" is 2 for normal and 3 for reversed. 
                        reversed = reader.NextInt() == 3;
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            return new CircleShape(name, position, size, reversed);
        }
    }
}
