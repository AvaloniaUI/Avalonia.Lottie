using System.Collections.Generic;
using Avalonia.Lottie.Model;
using Avalonia.Lottie.Model.Content;

namespace Avalonia.Lottie.Parser
{
    internal static class FontCharacterParser
    {
        internal static FontCharacter Parse(JsonReader reader, LottieComposition composition)
        {
            var character = '\0';
            double size = 0;
            double width = 0;
            string style = null;
            string fontFamily = null;
            var shapes = new List<ShapeGroup>();

            reader.BeginObject();
            while (reader.HasNext())
                switch (reader.NextName())
                {
                    case "ch":
                        character = reader.NextString()[0];
                        break;
                    case "size":
                        size = reader.NextDouble();
                        break;
                    case "w":
                        width = reader.NextDouble();
                        break;
                    case "style":
                        style = reader.NextString();
                        break;
                    case "fFamily":
                        fontFamily = reader.NextString();
                        break;
                    case "data":
                        reader.BeginObject();
                        while (reader.HasNext())
                            if ("shapes".Equals(reader.NextName()))
                            {
                                reader.BeginArray();
                                while (reader.HasNext())
                                    shapes.Add((ShapeGroup) ContentModelParser.Parse(reader, composition));
                                reader.EndArray();
                            }
                            else
                            {
                                reader.SkipValue();
                            }

                        reader.EndObject();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }

            reader.EndObject();

            return new FontCharacter(shapes, character, size, width, style, fontFamily);
        }
    }
}