using Avalonia.Lottie.Animation.Keyframe;

namespace Avalonia.Lottie.Model.Animatable
{
    public interface IAnimatableValue<out TK, TA>
    {
        IBaseKeyframeAnimation<TK, TA> CreateAnimation();
    }
}