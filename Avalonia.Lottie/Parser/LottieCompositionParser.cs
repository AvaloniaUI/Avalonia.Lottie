using System.Collections.Generic;

using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Lottie.Model;
using Avalonia.Lottie.Model.Layer;

namespace Avalonia.Lottie.Parser
{
    public static class LottieCompositionParser
    {
        public static LottieComposition Parse(JsonReader reader)
        {
            var scale = Utils.Utils.DpScale();
            var startFrame = 0f;
            var endFrame = 0f;
            var frameRate = 0f;
            var layerMap = new Dictionary<long, Layer>();
            var layers = new List<Layer>();
            var width = 0;
            var height = 0;
            var precomps = new Dictionary<string, List<Layer>>();
            var images = new Dictionary<string, LottieImageAsset>();
            var fonts = new Dictionary<string, Font>();
            var characters = new Dictionary<int, FontCharacter>();
            var composition = new LottieComposition();

            reader.BeginObject();
            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "w":
                        width = reader.NextInt();
                        break;
                    case "h":
                        height = reader.NextInt();
                        break;
                    case "ip":
                        startFrame = reader.NextDouble();
                        break;
                    case "op":
                        endFrame = reader.NextDouble() - 0.01f;
                        break;
                    case "fr":
                        frameRate = reader.NextDouble();
                        break;
                    case "v":
                        var version = reader.NextString();
                        var versions = Regex.Split(version, "\\.");
                        var majorVersion = int.Parse(versions[0]);
                        var minorVersion = int.Parse(versions[1]);
                        var patchVersion = int.Parse(versions[2]);
                        if (!Utils.Utils.IsAtLeastVersion(majorVersion, minorVersion, patchVersion, 4, 4, 0))
                        {
                            composition.AddWarning("Lottie only supports bodymovin >= 4.4.0");
                        }
                        break;
                    case "layers":
                        ParseLayers(reader, composition, layers, layerMap);
                        break;
                    case "assets":
                        ParseAssets(reader, composition, precomps, images);
                        break;
                    case "fonts":
                        ParseFonts(reader, fonts);
                        break;
                    case "chars":
                        ParseChars(reader, composition, characters);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }
            reader.EndObject();

            var scaledWidth = (int)(width * scale);
            var scaledHeight = (int)(height * scale);
            var bounds = new Rect(0, 0, scaledWidth, scaledHeight);

            composition.Init(bounds, startFrame, endFrame, frameRate, layers, layerMap, precomps, images, characters, fonts);

            return composition;
        }

        private static void ParseLayers(JsonReader reader, LottieComposition composition, List<Layer> layers, Dictionary<long, Layer> layerMap)
        {
            var imageCount = 0;
            reader.BeginArray();
            while (reader.HasNext())
            {
                var layer = LayerParser.Parse(reader, composition);
                if (layer.GetLayerType() == Layer.LayerType.Image)
                {
                    imageCount++;
                }
                layers.Add(layer);
                layerMap[layer.Id] = layer;

                if (imageCount > 4)
                {
                    LottieLog.Warn($"You have {imageCount} images. Lottie should primarily be used with shapes. If you are using Adobe Illustrator, convert the Illustrator layers to shape layers.");
                }
            }
            reader.EndArray();
        }

        private static void ParseAssets(JsonReader reader, LottieComposition composition, Dictionary<string, List<Layer>> precomps, Dictionary<string, LottieImageAsset> images)
        {
            reader.BeginArray();
            while (reader.HasNext())
            {
                string id = null;
                // For precomps
                var layers = new List<Layer>();
                var layerMap = new Dictionary<long, Layer>();
                // For images
                var width = 0;
                var height = 0;
                string imageFileName = null;
                string relativeFolder = null;
                reader.BeginObject();
                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "id":
                            id = reader.NextString();
                            break;
                        case "layers":
                            reader.BeginArray();
                            while (reader.HasNext())
                            {
                                var layer = LayerParser.Parse(reader, composition);
                                layerMap.Add(layer.Id, layer);
                                layers.Add(layer);
                            }
                            reader.EndArray();
                            break;
                        case "w":
                            width = reader.NextInt();
                            break;
                        case "h":
                            height = reader.NextInt();
                            break;
                        case "p":
                            imageFileName = reader.NextString();
                            break;
                        case "u":
                            relativeFolder = reader.NextString();
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }
                reader.EndObject();
                if (imageFileName != null)
                {
                    var image =
                        new LottieImageAsset(width, height, id, imageFileName, relativeFolder);
                    images[image.Id] = image;
                }
                else
                {
                    precomps.Add(id, layers);
                }
            }
            reader.EndArray();
        }

        private static void ParseFonts(JsonReader reader, Dictionary<string, Font> fonts)
        {
            reader.BeginObject();
            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "list":
                        reader.BeginArray();
                        while (reader.HasNext())
                        {
                            var font = FontParser.Parse(reader);
                            fonts.Add(font.Name, font);
                        }
                        reader.EndArray();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }
            reader.EndObject();
        }

        private static void ParseChars(JsonReader reader, LottieComposition composition, Dictionary<int, FontCharacter> characters)
        {
            reader.BeginArray();
            while (reader.HasNext())
            {
                var character = FontCharacterParser.Parse(reader, composition);
                characters.Add(character.GetHashCode(), character);
            }
            reader.EndArray();
        }
    }
}
