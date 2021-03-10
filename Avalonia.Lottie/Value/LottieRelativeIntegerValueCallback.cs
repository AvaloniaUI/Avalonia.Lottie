﻿using System;
using Avalonia.Lottie.Utils;

namespace Avalonia.Lottie.Value
{
    /// <summary>
    ///     <see cref="Value.LottieValueCallback{T}" /> that provides a value offset from the original animation
    ///     rather than an absolute value.
    /// </summary>
    public class LottieRelativeIntegerValueCallback : LottieValueCallback<int?>
    {
        public override int? GetValue(LottieFrameInfo<int?> frameInfo)
        {
            var originalValue = MiscUtils.Lerp(
                frameInfo.StartValue.Value,
                frameInfo.EndValue.Value,
                frameInfo.InterpolatedKeyframeProgress
            );
            var newValue = GetOffset(frameInfo);
            return originalValue + newValue;
        }

        /// <summary>
        ///     Override this to provide your own offset on every frame.
        /// </summary>
        /// <param name="frameInfo"></param>
        /// <returns></returns>
        public int GetOffset(LottieFrameInfo<int?> frameInfo)
        {
            if (Value == null)
                throw new ArgumentException("You must provide a static value in the constructor " +
                                            ", call setValue, or override getValue.");
            return Value.Value;
        }
    }
}