using System.Numerics;

namespace Avalonia.Lottie.Value
{
    public class Keyframe<T>
    {
        private readonly LottieComposition _composition;
        private double  _endProgress = double .MinValue;

        private double  _startProgress = double .MinValue;

        public Keyframe(LottieComposition composition, T startValue, T endValue, IInterpolator interpolator,
            double ? startFrame, double ? endFrame)
        {
            _composition = composition;
            StartValue = startValue;
            EndValue = endValue;
            Interpolator = interpolator;
            StartFrame = startFrame;
            EndFrame = endFrame;
        }

        /// <summary>
        ///     Non-animated value.
        /// </summary>
        /// <param name="value"></param>
        public Keyframe(T value)
        {
            _composition = null;
            StartValue = value;
            EndValue = value;
            Interpolator = null;
            StartFrame = double .MinValue;
            EndFrame = double .MaxValue;
        }

        public T StartValue { get; }
        public T EndValue { get; internal set; }
        public IInterpolator Interpolator { get; }
        public double ? StartFrame { get; }
        public double ? EndFrame { get; internal set; }

        // Used by PathKeyframe but it has to be parsed by KeyFrame because we use a JsonReader to 
        // deserialzie the data so we have to parse everything in order 
        public Vector? PathCp1 { get; set; }
        public Vector? PathCp2 { get; set; }

        public virtual double  StartProgress
        {
            get
            {
                if (_composition == null) return 0f;
                if (_startProgress == double .MinValue)
                    _startProgress = (StartFrame.Value - _composition.StartFrame) / _composition.DurationFrames;
                return _startProgress;
            }
        }

        public virtual double  EndProgress
        {
            get
            {
                if (_composition == null) return 1f;
                if (_endProgress == double .MinValue)
                {
                    if (EndFrame == null)
                    {
                        _endProgress = 1f;
                    }
                    else
                    {
                        var startProgress = StartProgress;
                        var durationFrames = EndFrame.Value - StartFrame.Value;
                        var durationProgress = durationFrames / _composition.DurationFrames;
                        _endProgress = startProgress + durationProgress;
                    }
                }

                return _endProgress;
            }
        }

        public virtual bool Static => Interpolator == null;

        public virtual bool ContainsProgress(double progress)
        {
            return progress >= StartProgress && progress < EndProgress;
        }

        public override string ToString()
        {
            return "Keyframe{" + "startValue=" + StartValue + ", endValue=" + EndValue + ", startFrame=" + StartFrame +
                   ", endFrame=" + EndFrame + ", interpolator=" + Interpolator + '}';
        }
    }
}