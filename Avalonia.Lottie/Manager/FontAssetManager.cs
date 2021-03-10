using System;
using System.Collections.Generic;
using Avalonia.Media;

namespace Avalonia.Lottie.Manager
{
    internal class FontAssetManager
    {
        /// <summary>
        ///     Map of font families to their fonts. Necessary to create a font with a different style
        /// </summary>
        private readonly Dictionary<string, Typeface> _fontFamilies = new();

        /// <summary>
        ///     Pair is (fontName, fontStyle)
        /// </summary>
        private readonly Dictionary<Tuple<string, string>, Typeface> _fontMap = new();

        private string _defaultFontFileExtension = ".ttf";
        private FontAssetDelegate _delegate;
        private Tuple<string, string> _tempPair;

        internal FontAssetManager(FontAssetDelegate @delegate)
        {
            _delegate = @delegate;
        }

        internal virtual FontAssetDelegate Delegate
        {
            set => _delegate = value;
        }

        /// <summary>
        ///     Sets the default file extension (include the `.`).
        ///     e.g. `.ttf` `.otf`
        ///     Defaults to `.ttf`
        /// </summary>
        public virtual string DefaultFontFileExtension
        {
            set => _defaultFontFileExtension = value;
        }

        internal virtual Typeface GetTypeface(string fontFamily, string style)
        {
            _tempPair = new Tuple<string, string>(fontFamily, style);
            if (_fontMap.TryGetValue(_tempPair, out var typeface)) return typeface;
            var typefaceWithDefaultStyle = GetFontFamily(fontFamily);
            typeface = TypefaceForStyle(typefaceWithDefaultStyle, style);
            _fontMap[_tempPair] = typeface;
            return typeface;
        }

        private Typeface GetFontFamily(string fontFamily)
        {
            if (_fontFamilies.TryGetValue(fontFamily, out var defaultTypeface)) return defaultTypeface;

            Typeface typeface = null;
            if (_delegate != null) typeface = _delegate.FetchFont(fontFamily);

            if (_delegate != null && typeface == null)
            {
                var path = _delegate.GetFontPath(fontFamily);
                if (!ReferenceEquals(path, null)) typeface = Typeface.CreateFromAsset(path);
            }

            if (typeface == null)
            {
                var path = $"Assets/Fonts/{fontFamily.Replace(" ", "")}{_defaultFontFileExtension}#{fontFamily}";
                typeface = Typeface.CreateFromAsset(path);
            }

            _fontFamilies[fontFamily] = typeface;
            return typeface;
        }

        private Typeface TypefaceForStyle(Typeface typeface, string style)
        {
            var containsItalic = style.Contains("Italic");
            var containsBold = style.Contains("Bold");

            var fontStyle = containsItalic ? FontStyle.Italic : FontStyle.Normal;
            var fontWeight = containsBold ? FontWeight.Bold : FontWeight.Normal;

            if (typeface.Style == fontStyle && typeface.Weight == fontWeight) return typeface;

            return Typeface.Create(typeface, fontStyle, fontWeight);
        }
    }
}