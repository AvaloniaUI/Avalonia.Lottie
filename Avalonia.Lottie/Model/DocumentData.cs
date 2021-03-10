using System;
using Avalonia.Media;

namespace Avalonia.Lottie.Model
{
    public class DocumentData
    {
        internal readonly double BaselineShift;
        internal readonly Color Color;
        internal readonly string FontName;
        internal readonly int Justification;
        internal readonly double LineHeight;
        internal readonly double Size;
        internal readonly Color StrokeColor;
        internal readonly bool StrokeOverFill;
        internal readonly int StrokeWidth;
        internal readonly string Text;
        internal readonly int Tracking;

        internal DocumentData(string text, string fontName, double size, int justification, int tracking,
            double lineHeight, double baselineShift, Color color, Color strokeColor, int strokeWidth,
            bool strokeOverFill)
        {
            Text = text;
            FontName = fontName;
            Size = size;
            Justification = justification;
            Tracking = tracking;
            LineHeight = lineHeight;
            BaselineShift = baselineShift;
            Color = color;
            StrokeColor = strokeColor;
            StrokeWidth = strokeWidth;
            StrokeOverFill = strokeOverFill;
        }

        public override int GetHashCode()
        {
            int result;
            long temp;
            result = Text.GetHashCode();
            result = 31 * result + FontName.GetHashCode();
            result = (int) (31 * result + Size);
            result = 31 * result + Justification;
            result = 31 * result + Tracking;
            temp = BitConverter.DoubleToInt64Bits(LineHeight);
            result = 31 * result + (int) (temp ^ (long) ((ulong) temp >> 32));
            result = 31 * result + Color.GetHashCode();
            return result;
        }
    }
}