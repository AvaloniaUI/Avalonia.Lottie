using System;
using System.Collections.Generic;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Animation.Keyframe
{
    internal class FloatKeyframeAnimation : KeyframeAnimation<float?>
    {
        internal FloatKeyframeAnimation(List<Keyframe<float?>> keyframes) : base(keyframes)
        {
        }

        public override float? GetValue(Keyframe<float?> keyframe, double  keyframeProgress)
        {
            if (keyframe.StartValue == null || keyframe.EndValue == null)
                throw new InvalidOperationException("Missing values for keyframe.");

            if (ValueCallback != null)
                return ValueCallback.GetValueInternal(keyframe.StartFrame.Value, keyframe.EndFrame.Value,
                    keyframe.StartValue, keyframe.EndValue, keyframeProgress, LinearCurrentKeyframeProgress, Progress);

            return (float?) MathExt.Lerp(keyframe.StartValue.Value, keyframe.EndValue.Value, keyframeProgress);
        }
    }
}