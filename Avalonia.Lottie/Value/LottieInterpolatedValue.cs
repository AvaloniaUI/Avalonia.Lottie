﻿namespace Avalonia.Lottie.Value
{
    public abstract class LottieInterpolatedValue<T> : LottieValueCallback<T>
    {
        private readonly T _endValue;
        private readonly IInterpolator _interpolator;
        private readonly T _startValue;

        protected LottieInterpolatedValue(T startValue, T endValue)
            : this(startValue, endValue, new LinearInterpolator())
        {
        }

        protected LottieInterpolatedValue(T startValue, T endValue, IInterpolator interpolator)
        {
            _startValue = startValue;
            _endValue = endValue;
            _interpolator = interpolator;
        }

        public override T GetValue(LottieFrameInfo<T> frameInfo)
        {
            var progress = _interpolator.GetInterpolation(frameInfo.OverallProgress);
            return InterpolateValue(_startValue, _endValue, progress);
        }

        protected abstract T InterpolateValue(T startValue, T endValue, double  progress);
    }
}