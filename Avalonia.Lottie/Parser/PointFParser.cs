/* Unmerged change from project 'Avalonia.Lottie (netcoreapp3.0)'
Before:

using Newtonsoft.Json;
After:
using Newtonsoft.Json;

*/

using System;
using System.Numerics;
using Newtonsoft.Json;

namespace Avalonia.Lottie.Parser
{
    internal class PointFParser : IValueParser<Vector?>
    {
        internal static readonly PointFParser Instance = new();

        private PointFParser()
        {
        }

        public Vector? Parse(JsonReader reader)
        {
            var token = reader.Peek();
            if (token == JsonToken.StartArray) return JsonUtils.JsonToPoint(reader);
            if (token == JsonToken.StartObject) return JsonUtils.JsonToPoint(reader);
            if (token == JsonToken.Integer || token == JsonToken.Float)
            {
                // This is the case where the static value for a property is an array of numbers. 
                // We begin the array to see if we have an array of keyframes but it's just an array 
                // of static numbers instead. 
                var point = new Vector((float) reader.NextDouble() , (float) reader.NextDouble());
                while (reader.HasNext()) reader.SkipValue();
                return point;
            }

            throw new ArgumentException("Cannot convert json to point. Next token is " + token);
        }
    }
}