using System.Collections.Generic;
using Avalonia.Lottie.Utils;
using Avalonia.Lottie.Value;
using Avalonia.Media;

namespace Avalonia.Lottie.Animation.Keyframe
{
    internal class ColorKeyframeAnimation : KeyframeAnimation<Color?>
    {
        internal ColorKeyframeAnimation(List<Keyframe<Color?>> keyframes) : base(keyframes)
        {
        }

        public override Color? GetValue(Keyframe<Color?> keyframe, float keyframeProgress)
        {
            if (keyframe.StartValue == null || keyframe.EndValue == null)
            {
                throw new System.InvalidOperationException("Missing values for keyframe.");
            }
            var startColor = keyframe.StartValue;
            var endColor = keyframe.EndValue;

            if (ValueCallback != null)
            {
                return ValueCallback.GetValueInternal(keyframe.StartFrame.Value, keyframe.EndFrame.Value, startColor, endColor, keyframeProgress, LinearCurrentKeyframeProgress, Progress);
            }

            return GammaEvaluator.Evaluate(keyframeProgress, startColor.Value, endColor.Value);
        }
    }
}