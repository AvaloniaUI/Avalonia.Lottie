﻿using System;
using System.Collections.Generic;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Animation.Keyframe
{
    public interface IBaseKeyframeAnimation
    {
        double  Progress { get; set; }
        event EventHandler ValueChanged;
        void OnValueChanged();
    }

    public interface IBaseKeyframeAnimation<out TK, TA> : IBaseKeyframeAnimation
    {
        TA Value { get; }
        void SetValueCallback(ILottieValueCallback<TA> valueCallback);
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="TK">Keyframe type</typeparam>
    /// <typeparam name="TA">Animation type</typeparam>
    public abstract class BaseKeyframeAnimation<TK, TA> : IBaseKeyframeAnimation<TK, TA>
    {
        private readonly List<Keyframe<TK>> _keyframes;

        private Keyframe<TK> _cachedKeyframe;
        private bool _isDiscrete;
        private double  _progress;
        protected ILottieValueCallback<TA> ValueCallback;

        internal BaseKeyframeAnimation(List<Keyframe<TK>> keyframes)
        {
            _keyframes = keyframes;
        }

        private Keyframe<TK> CurrentKeyframe
        {
            get
            {
                if (_keyframes.Count == 0) throw new InvalidOperationException("There are no keyframes");

                if (_cachedKeyframe != null && _cachedKeyframe.ContainsProgress(_progress)) return _cachedKeyframe;

                var keyframe = _keyframes[_keyframes.Count - 1];
                if (_progress < keyframe.StartProgress)
                    for (var i = _keyframes.Count - 1; i >= 0; i--)
                    {
                        keyframe = _keyframes[i];
                        if (keyframe.ContainsProgress(_progress)) break;
                    }

                _cachedKeyframe = keyframe;
                return keyframe;
            }
        }

        /// <summary>
        ///     Returns the progress into the current keyframe between 0 and 1. This does not take into account
        ///     any interpolation that the keyframe may have.
        /// </summary>
        protected double  LinearCurrentKeyframeProgress
        {
            get
            {
                if (_isDiscrete) return 0f;

                var keyframe = CurrentKeyframe;
                if (keyframe.Static) return 0f;
                var progressIntoFrame = _progress - keyframe.StartProgress;
                var keyframeProgress = keyframe.EndProgress - keyframe.StartProgress;
                return progressIntoFrame / keyframeProgress;
            }
        }

        /// Takes the value of
        /// <see cref="LinearCurrentKeyframeProgress" />
        /// and interpolates it with 
        /// the current keyframe's interpolator.
        private double  InterpolatedCurrentKeyframeProgress
        {
            get
            {
                var keyframe = CurrentKeyframe;
                if (keyframe.Static) return 0f;

                return keyframe.Interpolator.GetInterpolation(LinearCurrentKeyframeProgress);
            }
        }

        private double  StartDelayProgress
        {
            get
            {
                var startDelayProgress = _keyframes.Count == 0 ? 0f : _keyframes[0].StartProgress;
                if (startDelayProgress < 0)
                    return 0;
                if (startDelayProgress > 1)
                    return 1;
                return startDelayProgress;
            }
        }

        protected virtual double  EndProgress
        {
            get
            {
                var endProgress = _keyframes.Count == 0 ? 1f : _keyframes[_keyframes.Count - 1].EndProgress;
                if (endProgress < 0)
                    return 0;
                if (endProgress > 1)
                    return 1;
                return endProgress;
            }
        }

        public virtual event EventHandler ValueChanged;

        public virtual double  Progress
        {
            set
            {
                if (value < 0 || double .IsNaN(value))
                    value = 0;
                if (value > 1)
                    value = 1;

                if (value < StartDelayProgress)
                    value = StartDelayProgress;
                else if (value > EndProgress) value = EndProgress;

                if (value == _progress) return;
                _progress = value;

                OnValueChanged();
            }
            get => _progress;
        }

        public virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public virtual TA Value => GetValue(CurrentKeyframe, InterpolatedCurrentKeyframeProgress);

        public void SetValueCallback(ILottieValueCallback<TA> valueCallback)
        {
            ValueCallback?.SetAnimation(null);
            ValueCallback = valueCallback;
            valueCallback?.SetAnimation(this);
        }

        internal virtual void SetIsDiscrete()
        {
            _isDiscrete = true;
        }

        /// <summary>
        ///     keyframeProgress will be [0, 1] unless the interpolator has overshoot in which case, this
        ///     should be able to handle values outside of that range.
        /// </summary>
        public abstract TA GetValue(Keyframe<TK> keyframe, double  keyframeProgress);
    }
}