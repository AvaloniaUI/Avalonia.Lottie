using System;
using System.Diagnostics;

namespace Avalonia.Lottie.Utils
{
    /// <summary>
    ///     This is a slightly modified <seealso cref="ValueAnimator" /> that allows us to update start and end values
    ///     easily optimizing for the fact that we know that it's a value animator with 2 floats.
    /// </summary>
    public class LottieValueAnimator : BaseLottieAnimator
    {
        private LottieComposition _composition;
        private float _frame;
        private float _frameRate;
        private long _lastFrameTimeNs;
        private float _maxFrame = int.MaxValue;
        private float _minFrame = int.MinValue;
        private int _repeatCount;
        protected bool _running;
        private bool _speedReversedForRepeatMode;

        /// <summary>
        ///     Returns a float representing the current value of the animation from 0 to 1
        ///     regardless of the animation speed, direction, or min and max frames.
        /// </summary>
        public float AnimatedValue => AnimatedValueAbsolute;

        /// <summary>
        ///     Returns the current value of the animation from 0 to 1 regardless
        ///     of the animation speed, direction, or min and max frames.
        /// </summary>
        public float AnimatedValueAbsolute
        {
            get
            {
                if (_composition == null) return 0;
                return (_frame - _composition.StartFrame) / (_composition.EndFrame - _composition.StartFrame);
            }
        }

        /// <summary>
        ///     Returns the current value of the currently playing animation taking into
        ///     account direction, min and max frames.
        /// </summary>
        public override float AnimatedFraction
        {
            get
            {
                if (_composition == null) return 0;
                if (IsReversed)
                    return (MaxFrame - _frame) / (MaxFrame - MinFrame);
                return (_frame - MinFrame) / (MaxFrame - MinFrame);
            }
        }

        public override long Duration => _composition == null ? 0 : (long) _composition.Duration;

        public float Frame
        {
            get => _frame;
            set
            {
                if (_frame == value) return;
                _frame = MiscUtils.Clamp(value, MinFrame, MaxFrame);
                _lastFrameTimeNs = SystemnanoTime();
                OnAnimationUpdate();
            }
        }

        public override bool IsRunning => _running;

        private float FrameDurationNs
        {
            get
            {
                if (_composition == null) return float.MaxValue;
                return Utils.SecondInNanos / _composition.FrameRate / Math.Abs(Speed);
            }
        }

        public override float FrameRate
        {
            get => _frameRate;
            set
            {
                _frameRate = value <= 1000 ? value > 1 ? value : 1 : 1000;
                UpdateTimerInterval();
            }
        }

        public LottieComposition Composition
        {
            set
            {
                // Because the initial composition is loaded async, the first min/max frame may be set
                var keepMinAndMaxFrames = _composition == null;
                _composition = value;

                if (keepMinAndMaxFrames)
                    SetMinAndMaxFrames(
                        (int) Math.Max(_minFrame, _composition.StartFrame),
                        (int) Math.Min(_maxFrame, _composition.EndFrame)
                    );
                else
                    SetMinAndMaxFrames((int) _composition.StartFrame, (int) _composition.EndFrame);

                FrameRate = _composition.FrameRate;
                Frame = _frame;
                _lastFrameTimeNs = SystemnanoTime();
            }
        }

        public float MinFrame
        {
            get
            {
                if (_composition == null) return 0;
                return _minFrame == int.MinValue ? _composition.StartFrame : _minFrame;
            }
            set => SetMinAndMaxFrames(value, _maxFrame);
        }

        public float MaxFrame
        {
            get
            {
                if (_composition == null) return 0;
                return _maxFrame == int.MaxValue ? _composition.EndFrame : _maxFrame;
            }
            set => SetMinAndMaxFrames(_minFrame, value);
        }

        /// <summary>
        ///     Gets or sets the current speed. This will be affected by repeat mode <see cref="RepeatMode.Reverse" />.
        /// </summary>
        public float Speed { set; get; } = 1f;

        public override RepeatMode RepeatMode
        {
            set
            {
                base.RepeatMode = value;
                if (value != RepeatMode.Reverse && _speedReversedForRepeatMode)
                {
                    _speedReversedForRepeatMode = false;
                    ReverseAnimationSpeed();
                }
            }
        }

        private bool IsReversed => Speed < 0;

        public override void DoFrame()
        {
            base.DoFrame();
            PostFrameCallback();
            if (_composition == null || !IsRunning) return;

            var now = SystemnanoTime();
            var timeSinceFrame = now - _lastFrameTimeNs;
            var frameDuration = FrameDurationNs;
            var dFrames = timeSinceFrame / frameDuration;

            _frame += IsReversed ? -dFrames : dFrames;
            var ended = !MiscUtils.Contains(_frame, MinFrame, MaxFrame);
            _frame = MiscUtils.Clamp(_frame, MinFrame, MaxFrame);

            _lastFrameTimeNs = now;

            Debug.WriteLineIf(LottieLog.TraceEnabled, $"Tick milliseconds: {timeSinceFrame}", LottieLog.Tag);

            OnAnimationUpdate();
            if (ended)
            {
                if (RepeatCount != LottieDrawable.Infinite && _repeatCount >= RepeatCount)
                {
                    _frame = MaxFrame;
                    RemoveFrameCallback();
                    OnAnimationEnd(IsReversed);
                }
                else
                {
                    OnAnimationRepeat();
                    _repeatCount++;
                    if (RepeatMode == RepeatMode.Reverse)
                    {
                        _speedReversedForRepeatMode = !_speedReversedForRepeatMode;
                        ReverseAnimationSpeed();
                    }
                    else
                    {
                        _frame = IsReversed ? MaxFrame : MinFrame;
                    }

                    _lastFrameTimeNs = now;
                }
            }

            VerifyFrame();
        }

        public void ClearComposition()
        {
            _composition = null;
            _minFrame = int.MinValue;
            _maxFrame = int.MaxValue;
        }

        public void SetMinAndMaxFrames(float minFrame, float maxFrame)
        {
            var compositionMinFrame = _composition == null ? -float.MaxValue : _composition.StartFrame;
            var compositionMaxFrame = _composition == null ? float.MaxValue : _composition.EndFrame;
            _minFrame = MiscUtils.Clamp(minFrame, compositionMinFrame, compositionMaxFrame);
            _maxFrame = MiscUtils.Clamp(maxFrame, compositionMinFrame, compositionMaxFrame);
            Frame = MiscUtils.Clamp(_frame, minFrame, maxFrame);
        }

        public void ReverseAnimationSpeed()
        {
            Speed = -Speed;
        }

        public void PlayAnimation()
        {
            OnAnimationStart(IsReversed);
            Frame = IsReversed ? MaxFrame : MinFrame;
            _lastFrameTimeNs = SystemnanoTime();
            _repeatCount = 0;
            PostFrameCallback();
        }

        public void EndAnimation()
        {
            RemoveFrameCallback();
            OnAnimationEnd(IsReversed);
        }

        public void PauseAnimation()
        {
            RemoveFrameCallback();
        }

        public void ResumeAnimation()
        {
            PostFrameCallback();
            _lastFrameTimeNs = SystemnanoTime();
            if (IsReversed && Frame == MinFrame)
                _frame = MaxFrame;
            else if (!IsReversed && Frame == MaxFrame) _frame = MinFrame;
        }

        public override void Cancel()
        {
            OnAnimationCancel();
            RemoveFrameCallback();
        }

        protected virtual void PostFrameCallback()
        {
            PrivateStart();
            _running = true;
        }

        protected override void RemoveFrameCallback()
        {
            base.RemoveFrameCallback();
            _running = false;
        }

        private void VerifyFrame()
        {
            if (_composition == null) return;
            if (_frame < _minFrame || _frame > _maxFrame)
                throw new InvalidOperationException($"Frame must be [{_minFrame},{_maxFrame}]. It is {_frame}");
        }

        protected override void Disposing(bool disposing)
        {
            base.Disposing(disposing);
            _composition?.Dispose();
            _composition = null;
        }
    }
}