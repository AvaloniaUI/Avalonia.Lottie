namespace Avalonia.Lottie.Model
{
    public class Font
    {
        internal readonly double  Ascent;

        public Font(string family, string name, string style, double  ascent)
        {
            Family = family;
            Name = name;
            Style = style;
            Ascent = ascent;
        }

        public string Family { get; }

        public string Name { get; }

        public string Style { get; }
    }
}