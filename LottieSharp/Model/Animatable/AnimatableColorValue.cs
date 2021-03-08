﻿using LottieSharp.Animation.Keyframe;
using LottieSharp.Value;

using System.Collections.Generic;
using Avalonia.Media;

namespace LottieSharp.Model.Animatable
{
    public class AnimatableColorValue : BaseAnimatableValue<Color?, Color?>
    {
        public AnimatableColorValue(List<Keyframe<Color?>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<Color?, Color?> CreateAnimation()
        {
            return new ColorKeyframeAnimation(Keyframes);
        }
    }
}