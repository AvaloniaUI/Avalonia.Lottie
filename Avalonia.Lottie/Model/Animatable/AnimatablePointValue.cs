using System.Collections.Generic;
using System.Numerics;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Model.Animatable
{
    public class AnimatablePointValue : BaseAnimatableValue<Vector?, Vector?>
    {
        public AnimatablePointValue(List<Keyframe<Vector?>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<Vector?, Vector?> CreateAnimation()
        {
            return new PointKeyframeAnimation(Keyframes);
        }
    }
}