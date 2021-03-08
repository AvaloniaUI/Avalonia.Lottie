using LottieSharp.Model.Content;
using LottieSharp.Utils;
using Newtonsoft.Json;

using System.Collections.Generic;
using Avalonia.Media;

namespace LottieSharp.Parser
{
    public class GradientColorParser : IValueParser<GradientColor>
    {
        /** The number of colors if it exists in the json or -1 if it doesn't (legacy bodymovin) */
        private int _colorPoints;

        public GradientColorParser(int colorPoints)
        {
            _colorPoints = colorPoints;
        }

        /// <summary>
        /// Both the color stops and opacity stops are in the same array. 
        /// There are <see cref="_colorPoints"/> colors sequentially as: 
        /// [ 
        ///     ..., 
        ///     position, 
        ///     red, 
        ///     green, 
        ///     blue, 
        ///     ... 
        /// ] 
        /// 
        /// The remainder of the array is the opacity stops sequentially as: 
        /// [ 
        ///     ..., 
        ///     position, 
        ///     opacity, 
        ///     ... 
        /// ] 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public GradientColor Parse(JsonReader reader, float scale)
        {
            var array = new List<float>();
            // The array was started by Keyframe because it thought that this may be an array of keyframes 
            // but peek returned a number so it considered it a static array of numbers. 
            var isArray = reader.Peek() == JsonToken.StartArray;
            if (isArray)
            {
                reader.BeginArray();
            }
            while (reader.HasNext())
            {
                array.Add(reader.NextDouble());
            }
            if (isArray)
            {
                reader.EndArray();
            }
            if (_colorPoints == -1)
            {
                _colorPoints = array.Count / 4;
            }

            var positions = new float[_colorPoints];
            var colors = new Color[_colorPoints];

            byte r = 0;
            byte g = 0;
            for (var i = 0; i < _colorPoints * 4; i++)
            {
                var colorIndex = i / 4;
                double value = array[i];
                switch (i % 4)
                {
                    case 0:
                        // position 
                        positions[colorIndex] = (float)value;
                        break;
                    case 1:
                        r = (byte)(value * 255);
                        break;
                    case 2:
                        g = (byte)(value * 255);
                        break;
                    case 3:
                        var b = (byte)(value * 255);
                        colors[colorIndex] = new Color((byte)255, r, g, b);
                        break;
                }
            }

            var gradientColor = new GradientColor(positions, colors);
            AddOpacityStopsToGradientIfNeeded(gradientColor, array);
            return gradientColor;
        }

        /** 
       * This cheats a little bit. 
       * Opacity stops can be at arbitrary intervals independent of color stops. 
       * This uses the existing color stops and modifies the opacity at each existing color stop 
       * based on what the opacity would be. 
       * 
       * This should be a good approximation is nearly all cases. However, if there are many more 
       * opacity stops than color stops, information will be lost. 
       */
        private void AddOpacityStopsToGradientIfNeeded(GradientColor gradientColor, List<float> array)
        {
            var startIndex = _colorPoints * 4;
            if (array.Count <= startIndex)
            {
                return;
            }

            var opacityStops = (array.Count - startIndex) / 2;
            var positions = new double[opacityStops];
            var opacities = new double[opacityStops];

            for (int i = startIndex, j = 0; i < array.Count; i++)
            {
                if (i % 2 == 0)
                {
                    positions[j] = array[i];
                }
                else
                {
                    opacities[j] = array[i];
                    j++;
                }
            }

            for (var i = 0; i < gradientColor.Size; i++)
            {
                var color = gradientColor.Colors[i];
                color = new Color(GetOpacityAtPosition(gradientColor.Positions[i], positions, opacities),
                    color.R,
                    color.G,
                    color.B
                );
                gradientColor.Colors[i] = color;
            }
        }

        private byte GetOpacityAtPosition(double position, double[] positions, double[] opacities)
        {
            for (var i = 1; i < positions.Length; i++)
            {
                var lastPosition = positions[i - 1];
                var thisPosition = positions[i];
                if (positions[i] >= position)
                {
                    var progress = (position - lastPosition) / (thisPosition - lastPosition);
                    return (byte)(255 * MiscUtils.Lerp(opacities[i - 1], opacities[i], progress));
                }
            }
            return (byte)(255 * opacities[opacities.Length - 1]);
        }
    }
}
