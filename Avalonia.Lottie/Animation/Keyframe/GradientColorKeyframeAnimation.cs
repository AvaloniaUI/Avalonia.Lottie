using System.Collections.Generic;
using Avalonia.Lottie.Model.Content;
using Avalonia.Lottie.Value;
using Avalonia.Media;

namespace Avalonia.Lottie.Animation.Keyframe
{
    internal class GradientColorKeyframeAnimation : KeyframeAnimation<GradientColor>
    {
        private readonly GradientColor _gradientColor;

        internal GradientColorKeyframeAnimation(List<Keyframe<GradientColor>> keyframes) : base(keyframes)
        {
            var startValue = keyframes[0].StartValue;
            var size = startValue?.Size ?? 0;
            _gradientColor = new GradientColor(new double [size], new Color[size]);
        }

        public override GradientColor GetValue(Keyframe<GradientColor> keyframe, double  keyframeProgress)
        {
            _gradientColor.Lerp(keyframe.StartValue, keyframe.EndValue, keyframeProgress);
            return _gradientColor;
        }
    }
}