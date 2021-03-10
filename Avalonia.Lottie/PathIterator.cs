namespace Avalonia.Lottie
{
    public abstract class PathIterator
    {
        public enum ContourType
        {
            Arc,
            MoveTo,
            Line,
            Close,
            Bezier
        }

        public abstract bool Done { get; }

        public abstract bool Next();

        public abstract ContourType CurrentSegment(float[] points);
    }
}