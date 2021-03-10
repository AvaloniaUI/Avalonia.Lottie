﻿using System.Collections.Generic;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Animation.Keyframe
{
    internal class ValueCallbackKeyframeAnimation<TK, TA> : BaseKeyframeAnimation<TK, TA>
    {
        private readonly LottieFrameInfo<TA> _frameInfo = new();

        internal ValueCallbackKeyframeAnimation(ILottieValueCallback<TA> valueCallback) : base(new List<Keyframe<TK>>())
        {
            SetValueCallback(valueCallback);
        }

        /// <summary>
        ///     If this doesn't return 1, then <see cref="set_Progress" /> will always clamp the progress
        ///     to 0.
        /// </summary>
        protected override float EndProgress => 1f;

        public override TA Value =>
            ValueCallback.GetValueInternal(0f, 0f, default, default, Progress, Progress, Progress);

        public override void OnValueChanged()
        {
            if (ValueCallback != null) base.OnValueChanged();
        }

        public override TA GetValue(Keyframe<TK> keyframe, float keyframeProgress)
        {
            return Value;
        }
    }
}