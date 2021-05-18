namespace Avalonia.Lottie.Value
{
    /// <summary>
    ///     Data class for use with <see cref="LottieValueCallback{T}" />.
    ///     You should* not* hold a reference to the frame info parameter passed to your callback. It will be reused.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LottieFrameInfo<T>
    {
        public double  StartFrame { get; private set; }

        public double  EndFrame { get; private set; }

        public T StartValue { get; private set; }

        public T EndValue { get; private set; }

        public double  LinearKeyframeProgress { get; private set; }

        public double  InterpolatedKeyframeProgress { get; private set; }

        public double  OverallProgress { get; private set; }

        internal LottieFrameInfo<T> Set(
            double  startFrame,
            double  endFrame,
            T startValue,
            T endValue,
            double  linearKeyframeProgress,
            double  interpolatedKeyframeProgress,
            double  overallProgress
        )
        {
            StartFrame = startFrame;
            EndFrame = endFrame;
            StartValue = startValue;
            EndValue = endValue;
            LinearKeyframeProgress = linearKeyframeProgress;
            InterpolatedKeyframeProgress = interpolatedKeyframeProgress;
            OverallProgress = overallProgress;
            return this;
        }
    }
}