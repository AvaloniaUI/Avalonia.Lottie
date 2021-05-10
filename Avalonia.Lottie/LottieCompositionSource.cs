using System.ComponentModel;
using JetBrains.Annotations;

namespace Avalonia.Lottie
{
    [TypeConverter(typeof(LottieCompositionSourceTypeConverter))]
    public class LottieCompositionSource
    {
        [CanBeNull] public LottieComposition Composition { get; set; }
    }
}