using System.Collections.Generic;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Model.Animatable
{
    public class AnimatableIntegerValue : BaseAnimatableValue<int?, int?>
    {
        public AnimatableIntegerValue() : base(100)
        {
        }

        public AnimatableIntegerValue(List<Keyframe<int?>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<int?, int?> CreateAnimation()
        {
            return new IntegerKeyframeAnimation(Keyframes);
        }
    }
}