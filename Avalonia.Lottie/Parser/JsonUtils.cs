using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia.Media;
using Newtonsoft.Json;

namespace Avalonia.Lottie.Parser
{
    internal static class JsonUtils
    {
        /// <summary>
        ///     [r,g,b]
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal static Color JsonToColor(JsonReader reader)
        {
            reader.BeginArray();
            var r = (byte) (reader.NextDouble() * 255);
            var g = (byte) (reader.NextDouble() * 255);
            var b = (byte) (reader.NextDouble() * 255);
            while (reader.HasNext()) reader.SkipValue();
            reader.EndArray();
            return new Color(255, r, g, b);
        }

        internal static List<Vector> JsonToPoints(JsonReader reader, double  scale)
        {
            var points = new List<Vector>();

            reader.BeginArray();
            while (reader.Peek() == JsonToken.StartArray)
            {
                reader.BeginArray();
                points.Add(JsonToPoint(reader, scale));
                reader.EndArray();
            }

            reader.EndArray();
            return points;
        }

        internal static Vector JsonToPoint(JsonReader reader, double  scale)
        {
            switch (reader.Peek())
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                    return JsonNumbersToPoint(reader, scale);
                case JsonToken.StartArray: return JsonArrayToPoint(reader, scale);
                case JsonToken.StartObject: return JsonObjectToPoint(reader, scale);
                default: throw new ArgumentException("Unknown point starts with " + reader.Peek());
            }
        }

        private static Vector JsonNumbersToPoint(JsonReader reader, double  scale)
        {
            var x = reader.NextDouble();
            var y = reader.NextDouble();
            while (reader.HasNext()) reader.SkipValue();
            return new Vector((float)x * (float)scale, (float)y * (float)scale);
        }

        private static Vector JsonArrayToPoint(JsonReader reader, double  scale)
        {
            double  x;
            double  y;
            reader.BeginArray();
            x = reader.NextDouble();
            y = reader.NextDouble();
            while (reader.Peek() != JsonToken.EndArray) reader.SkipValue();
            reader.EndArray();
            return new Vector((float)x * (float)scale, (float)y * (float)scale);
        }

        private static Vector JsonObjectToPoint(JsonReader reader, double  scale)
        {
            var x = 0d;
            var y = 0d;
            reader.BeginObject();
            while (reader.HasNext())
                switch (reader.NextName())
                {
                    case "x":
                        x = ValueFromObject(reader);
                        break;
                    case "y":
                        y = ValueFromObject(reader);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }

            reader.EndObject();
            return new Vector((float)x * (float)scale, (float)y * (float)scale);
        }

        internal static double  ValueFromObject(JsonReader reader)
        {
            var token = reader.Peek();
            switch (token)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                    return reader.NextDouble();
                case JsonToken.StartArray:
                    reader.BeginArray();
                    var val = reader.NextDouble();
                    while (reader.HasNext()) reader.SkipValue();
                    reader.EndArray();
                    return val;
                default:
                    throw new ArgumentException("Unknown value for token of type " + token);
            }
        }
    }
}