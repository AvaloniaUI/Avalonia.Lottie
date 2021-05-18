using System.Collections.Generic;
using System.Numerics;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Animation.Keyframe
{
    internal class SplitDimensionPathKeyframeAnimation : BaseKeyframeAnimation<Vector?, Vector?>
    {
        private readonly IBaseKeyframeAnimation<float?, float?> _xAnimation;
        private readonly IBaseKeyframeAnimation<float?, float?> _yAnimation;
        private Vector _point;

        internal SplitDimensionPathKeyframeAnimation(IBaseKeyframeAnimation<float?, float?> xAnimation,
            IBaseKeyframeAnimation<float?, float?> yAnimation)
            : base(new List<Keyframe<Vector?>>())
        {
            _xAnimation = xAnimation;
            _yAnimation = yAnimation;
            // We need to call an initial setProgress so point gets set with the initial value. 
            Progress = Progress;
        }

        public override double  Progress
        {
            set
            {
                _xAnimation.Progress = value;
                _yAnimation.Progress = value;
                _point = new Vector(_xAnimation.Value ?? 0, _yAnimation.Value ?? 0);
                 OnValueChanged();
            }
        }

        public override Vector? Value => GetValue(null, 0);

        public override Vector? GetValue(Keyframe<Vector?> keyframe, double  keyframeProgress)
        {
            return _point;
        }
    }
}