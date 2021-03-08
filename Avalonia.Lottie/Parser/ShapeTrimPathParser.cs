using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Content;

namespace Avalonia.Lottie.Parser
{
    static class ShapeTrimPathParser
    {
        internal static ShapeTrimPath Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            var type = ShapeTrimPath.Type.Simultaneously;
            AnimatableFloatValue start = null;
            AnimatableFloatValue end = null;
            AnimatableFloatValue offset = null;

            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "s":
                        start = AnimatableValueParser.ParseFloat(reader, composition, false);
                        break;
                    case "e":
                        end = AnimatableValueParser.ParseFloat(reader, composition, false);
                        break;
                    case "o":
                        offset = AnimatableValueParser.ParseFloat(reader, composition, false);
                        break;
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "m":
                        type = (ShapeTrimPath.Type)reader.NextInt();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            return new ShapeTrimPath(name, type, start, end, offset);
        }
    }
}
