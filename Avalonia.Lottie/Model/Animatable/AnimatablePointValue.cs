﻿using System.Collections.Generic;
using System.Numerics;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Model.Animatable
{
    public class AnimatablePointValue : BaseAnimatableValue<Vector2?, Vector2?>
    {
        public AnimatablePointValue(List<Keyframe<Vector2?>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<Vector2?, Vector2?> CreateAnimation()
        {
            return new PointKeyframeAnimation(Keyframes);
        }
    }
}