using Avalonia.Lottie.Animation.Keyframe;

namespace Avalonia.Lottie.Value
{
    public interface ILottieValueCallback<T>
    {
        void SetAnimation(IBaseKeyframeAnimation animation);

        T GetValue(LottieFrameInfo<T> frameInfo);

        T GetValueInternal(
            float startFrame,
            float endFrame,
            T startValue,
            T endValue,
            float linearKeyframeProgress,
            float interpolatedKeyframeProgress,
            float overallProgress
        );
    }
}