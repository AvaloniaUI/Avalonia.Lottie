﻿using Avalonia.Lottie.Utils;

namespace Avalonia.Lottie.Value
{
    // ReSharper disable once UnusedMember.Global
    public class LottieInterpolatedFloatValue : LottieInterpolatedValue<float>
    {
        public LottieInterpolatedFloatValue(float startValue, float  endValue)
            : base(startValue, endValue)
        {
        }

        public LottieInterpolatedFloatValue(float startValue, float  endValue, IInterpolator interpolator)
            : base(startValue, endValue, interpolator)
        {
        }

        protected override float  InterpolateValue(float startValue, float  endValue, double  progress)
        {
            return (float) MiscUtils.Lerp(startValue, endValue, progress);
        }
    }
}