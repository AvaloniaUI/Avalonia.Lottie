using System.Collections.Generic;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Model.Animatable
{
    public class AnimatableTextFrame : BaseAnimatableValue<DocumentData, DocumentData>
    {
        public AnimatableTextFrame(List<Keyframe<DocumentData>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<DocumentData, DocumentData> CreateAnimation()
        {
            return new TextKeyframeAnimation(Keyframes);
        }
    }
}
