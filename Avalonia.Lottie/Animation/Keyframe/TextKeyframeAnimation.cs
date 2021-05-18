using System.Collections.Generic;
using Avalonia.Lottie.Model;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Animation.Keyframe
{
    internal class TextKeyframeAnimation : KeyframeAnimation<DocumentData>
    {
        internal TextKeyframeAnimation(List<Keyframe<DocumentData>> keyframes) : base(keyframes)
        {
        }

        public override DocumentData GetValue(Keyframe<DocumentData> keyframe, double  keyframeProgress)
        {
            return keyframe.StartValue;
        }
    }
}