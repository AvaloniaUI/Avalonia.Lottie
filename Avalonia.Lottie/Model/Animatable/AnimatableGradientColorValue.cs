using System.Collections.Generic;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Model.Content;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Model.Animatable
{
    public class AnimatableGradientColorValue : BaseAnimatableValue<GradientColor, GradientColor>
    {
        public AnimatableGradientColorValue(List<Keyframe<GradientColor>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<GradientColor, GradientColor> CreateAnimation()
        {
            return new GradientColorKeyframeAnimation(Keyframes);
        }
    }
}