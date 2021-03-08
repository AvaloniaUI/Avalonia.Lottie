using System.Collections.Generic;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Model.Content;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Model.Animatable
{
    public class AnimatableShapeValue : BaseAnimatableValue<ShapeData, Path>
    {
        public AnimatableShapeValue(List<Keyframe<ShapeData>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<ShapeData, Path> CreateAnimation()
        {
            return new ShapeKeyframeAnimation(Keyframes);
        }
    }
}