using System.Collections.Generic;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Animation.Keyframe
{
    internal abstract class KeyframeAnimation<T> : BaseKeyframeAnimation<T, T>
    {
        internal KeyframeAnimation(List<Keyframe<T>> keyframes) : base(keyframes)
        {
        }
    }
}