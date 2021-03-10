using System.Collections.Generic;
using Avalonia.Lottie.Model.Content;

namespace Avalonia.Lottie.Parser
{
    internal static class ShapeGroupParser
    {
        internal static ShapeGroup Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            var items = new List<IContentModel>();

            while (reader.HasNext())
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "it":
                        reader.BeginArray();
                        while (reader.HasNext())
                        {
                            var newItem = ContentModelParser.Parse(reader, composition);
                            if (newItem != null) items.Add(newItem);
                        }

                        reader.EndArray();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }

            return new ShapeGroup(name, items);
        }
    }
}