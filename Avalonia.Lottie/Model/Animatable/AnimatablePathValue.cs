using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Model.Animatable
{
    public class AnimatablePathValue : IAnimatableValue<Vector?, Vector?>
    {
        private readonly List<Keyframe<Vector?>> _keyframes;

        /// <summary>
        ///     Create a default static animatable path.
        /// </summary>
        public AnimatablePathValue()
        {
            _keyframes = new List<Keyframe<Vector?>> {new(new Vector(0, 0))};
        }

        public AnimatablePathValue(List<Keyframe<Vector?>> keyframes)
        {
            _keyframes = keyframes;
        }

        public IBaseKeyframeAnimation<Vector?, Vector?> CreateAnimation()
        {
            if (_keyframes[0].Static) return new PointKeyframeAnimation(_keyframes);

            return new PathKeyframeAnimation(_keyframes.ToList());
        }
    }
}