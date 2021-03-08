using Avalonia.Media;


namespace Avalonia.Lottie
{
    public class Typeface
    {
        private Typeface(string fontFamily, FontStyle style, FontWeight weight)
        {
            FontFamily = fontFamily;
            Style = style;
            Weight = weight;
        }

        public string FontFamily { get; }
        public FontStyle Style { get; }
        public FontWeight Weight { get; }

        public static Typeface Create(Typeface typeface, FontStyle style, FontWeight weight)
        {
            return new(typeface.FontFamily, style, weight);
        }

        public static Typeface CreateFromAsset(string path)
        {
            return new(path, FontStyle.Normal, FontWeight.Normal);
        }
    }
}