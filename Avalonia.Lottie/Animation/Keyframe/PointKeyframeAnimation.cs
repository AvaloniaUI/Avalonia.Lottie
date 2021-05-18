using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Animation.Keyframe
{
    internal class PointKeyframeAnimation : KeyframeAnimation<Vector?>
    {
        private Vector _point;

        internal PointKeyframeAnimation(List<Keyframe<Vector?>> keyframes) : base(keyframes)
        {
        }

        public override Vector? GetValue(Keyframe<Vector?> keyframe, double  keyframeProgress)
        {
            if (keyframe.StartValue == null || keyframe.EndValue == null)
                throw new InvalidOperationException("Missing values for keyframe.");

            var startPoint = keyframe.StartValue;
            var endPoint = keyframe.EndValue;

            if (ValueCallback != null)
                return ValueCallback.GetValueInternal(keyframe.StartFrame.Value, keyframe.EndFrame.Value, startPoint,
                    endPoint, keyframeProgress, LinearCurrentKeyframeProgress, Progress);
            
            _point = new Vector(startPoint.Value.X + keyframeProgress * (endPoint.Value.X - startPoint.Value.X),
                startPoint.Value.Y + keyframeProgress * (endPoint.Value.Y - startPoint.Value.Y));
             
            return _point;
        }
    }
}