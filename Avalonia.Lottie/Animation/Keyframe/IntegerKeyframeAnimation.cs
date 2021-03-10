using System;
using System.Collections.Generic;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Animation.Keyframe
{
    internal class IntegerKeyframeAnimation : KeyframeAnimation<int?>
    {
        internal IntegerKeyframeAnimation(List<Keyframe<int?>> keyframes) : base(keyframes)
        {
        }

        public override int? GetValue(Keyframe<int?> keyframe, float keyframeProgress)
        {
            if (keyframe.StartValue == null || keyframe.EndValue == null)
                throw new InvalidOperationException("Missing values for keyframe.");

            if (ValueCallback != null)
                return ValueCallback.GetValueInternal(keyframe.StartFrame.Value, keyframe.EndFrame.Value,
                    keyframe.StartValue, keyframe.EndValue, keyframeProgress, LinearCurrentKeyframeProgress, Progress);

            return (int?) MathExt.Lerp(keyframe.StartValue.Value, keyframe.EndValue.Value, keyframeProgress);
        }
    }
}