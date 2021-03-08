using System;

namespace Avalonia.Lottie
{
    public class PathInterpolator : IInterpolator
    {
        public PathInterpolator(float controlX1, float controlY1, float controlX2, float controlY2)
        {
            InitCubic(controlX1, controlY1, controlX2, controlY2);
        }

        private static readonly float Precision = 0.002f;

        private float[] _mX; // x coordinates in the line

        private float[] _mY; // y coordinates in the line

        private void InitCubic(float x1, float y1, float x2, float y2)
        {
            var path = new Path();
            path.MoveTo(0, 0);
            path.CubicTo(x1, y1, x2, y2, 1f, 1f);
            InitPath(path);
        }

        private void InitPath(Path path)
        {
            var pointComponents = path.Approximate(Precision);

            var numPoints = pointComponents.Length / 3;
            if (pointComponents[1] != 0 || pointComponents[2] != 0
                                        || pointComponents[pointComponents.Length - 2] != 1
                                        || pointComponents[pointComponents.Length - 1] != 1)
            {
                //throw new ArgumentException("The Path must start at (0,0) and end at (1,1)");
            }

            _mX = new float[numPoints];
            _mY = new float[numPoints];
            float prevX = 0;
            float prevFraction = 0;
            var componentIndex = 0;
            for (var i = 0; i < numPoints; i++)
            {
                var fraction = pointComponents[componentIndex++];
                var x = pointComponents[componentIndex++];
                var y = pointComponents[componentIndex++];
                if (fraction == prevFraction && x != prevX)
                {
                    throw new ArgumentException("The Path cannot have discontinuity in the X axis.");
                }
                if (x < prevX)
                {
                    //throw new ArgumentException("The Path cannot loop back on itself.");
                }
                _mX[i] = x;
                _mY[i] = y;
                prevX = x;
                prevFraction = fraction;
            }
        }

        public float GetInterpolation(float t)
        {
            if (t <= 0 || float.IsNaN(t))
            {
                return 0;
            }
            if (t >= 1)
            {
                return 1;
            }
            // Do a binary search for the corRect x to interpolate between.
            var startIndex = 0;
            var endIndex = _mX.Length - 1;

            while (endIndex - startIndex > 1)
            {
                var midIndex = (startIndex + endIndex) / 2;
                if (t < _mX[midIndex])
                {
                    endIndex = midIndex;
                }
                else
                {
                    startIndex = midIndex;
                }
            }

            var xRange = _mX[endIndex] - _mX[startIndex];
            if (xRange == 0)
            {
                return _mY[startIndex];
            }

            var tInRange = t - _mX[startIndex];
            var fraction = tInRange / xRange;

            var startY = _mY[startIndex];
            var endY = _mY[endIndex];
            return startY + (fraction * (endY - startY));
        }
    }
}