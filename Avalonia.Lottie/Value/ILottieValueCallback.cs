using Avalonia.Lottie.Animation.Keyframe;

namespace Avalonia.Lottie.Value
{
    public interface ILottieValueCallback<T>
    {
        void SetAnimation(IBaseKeyframeAnimation animation);

        T GetValue(LottieFrameInfo<T> frameInfo);

        T GetValueInternal(
            double  startFrame,
            double  endFrame,
            T startValue,
            T endValue,
            double  linearKeyframeProgress,
            double  interpolatedKeyframeProgress,
            double  overallProgress
        );
    }
}