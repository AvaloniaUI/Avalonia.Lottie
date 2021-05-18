using System;
using System.Numerics;
using Avalonia.Lottie.Utils;

namespace Avalonia.Lottie.Value
{
    /// <summary>
    ///     <see cref="Value.LottieValueCallback{T}" /> that provides a value offset from the original animation
    ///     rather than an absolute value.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LottieRelativePointValueCallback : LottieValueCallback<Vector?>
    {
        public LottieRelativePointValueCallback()
        {
        }

        public LottieRelativePointValueCallback(Vector staticValue)
            : base(staticValue)
        {
        }

        public override Vector? GetValue(LottieFrameInfo<Vector?> frameInfo)
        {
            var point = new Vector(
                 MiscUtils.Lerp(
                    frameInfo.StartValue.Value.X,
                    frameInfo.EndValue.Value.X,
                    frameInfo.InterpolatedKeyframeProgress),
                  MiscUtils.Lerp(
                    frameInfo.StartValue.Value.Y,
                    frameInfo.EndValue.Value.Y,
                    frameInfo.InterpolatedKeyframeProgress)
            );

            var offset = GetOffset(frameInfo);
            point += offset; 
            return point;
        }

        /// <summary>
        ///     Override this to provide your own offset on every frame.
        /// </summary>
        /// <param name="frameInfo"></param>
        /// <returns></returns>
        public Vector GetOffset(LottieFrameInfo<Vector?> frameInfo)
        {
            if (Value == null)
                throw new ArgumentException("You must provide a static value in the constructor " +
                                            ", call setValue, or override getValue.");
            return Value.Value;
        }
    }
}