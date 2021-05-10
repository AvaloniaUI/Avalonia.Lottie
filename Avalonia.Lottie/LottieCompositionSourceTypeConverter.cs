using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using Avalonia.Platform;

namespace Avalonia.Lottie
{
    public class LottieCompositionSourceTypeConverter : TypeConverter
    {
        private static readonly IAssetLoader s_AssetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();

        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var s = (string) value;

            if (s is { })
            {
                var uri = s.StartsWith("/")
                    ? new Uri(s, UriKind.Relative)
                    : new Uri(s, UriKind.RelativeOrAbsolute);

                LottieCompositionSource result = new LottieCompositionSource();

                if (uri.IsAbsoluteUri && uri.IsFile)
                {
                    using (var file = File.Open(uri.LocalPath, FileMode.Open, FileAccess.Read))
                    {
                        result.Composition = LottieCompositionFactory.FromJsonInputStreamSync(file, uri.AbsoluteUri).Value;
                    }
                }
                else
                {
                    using (var asset = s_AssetLoader.Open(uri))
                    {
                        result.Composition = LottieCompositionFactory.FromJsonInputStreamSync(asset, uri.AbsoluteUri).Value;
                    }
                }

                return result;
            }

            return null;
        }
    }
}