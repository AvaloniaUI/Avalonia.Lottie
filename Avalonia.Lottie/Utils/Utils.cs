using System;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Media;

//TODO: OID: Totally replaced with new file

namespace Avalonia.Lottie.Utils
{
    
    public static class Utils
    {
        public const int SecondInNanos = 1000000000;
        private static  Path _tempPath = new();
        private static  Path _tempPath2 = new();
        private static Vector[] _points = new Vector[2];
        private static readonly double  Sqrt2 =  Math.Sqrt(2);

        internal static Path CreatePath(Vector startPoint, Vector endPoint, Vector? cp1, Vector? cp2)
        {
            var path = new Path();
            path.MoveTo(startPoint.X, startPoint.Y);

            if (cp1.HasValue && cp2.HasValue && (cp1.Value.SquaredLength != 0 || cp2.Value.SquaredLength != 0))
                path.CubicTo(startPoint.X + cp1.Value.X, startPoint.Y + cp1.Value.Y, endPoint.X + cp2.Value.X,
                    endPoint.Y + cp2.Value.Y, endPoint.X, endPoint.Y);
            else
                path.LineTo(endPoint.X, endPoint.Y);
            return path;
        }

        public static void CloseQuietly(this IDisposable closeable)
        {
            if (closeable != null)
                try
                {
                    closeable.Dispose();
                }
                //catch (RuntimeException rethrown)
                //{
                //    throw rethrown;
                //}
                catch (Exception)
                {
                    // Really quietly
                }
        }

        internal static double  GetScale(Matrix matrix)
        {
            _points[0] = Vector.Zero;
            _points[1] = new Vector(Sqrt2, Sqrt2);
            
             // Use sqrt(2) so that the hypotenuse is of length 1.
            matrix.MapPoints(ref _points);
            var dx = _points[1].X - _points[0].X;
            var dy = _points[1].Y - _points[0].Y;

            // TODO: figure out why the result needs to be divided by 2.
            return  MathExt.Hypot(dx, dy) / 2f;
        }

        internal static void ApplyTrimPathIfNeeded(Path path, TrimPathContent trimPath)
        {
            if (trimPath == null) return;
            ApplyTrimPathIfNeeded(path, trimPath.Start.Value.Value / 100f, trimPath.End.Value.Value / 100f,
                trimPath.Offset.Value.Value / 360f);
        }

        public static double Distance(Vector v1, Vector v2)
        {
            var dV = v2 - v1;
            return dV.Length;
        }

        internal static void ApplyTrimPathIfNeeded(Path path, double  startValue, double  endValue, double  offsetValue)
        {
            LottieLog.BeginSection("applyTrimPathIfNeeded");
            using (var pathMeasure = new PathMeasure(path))
            {
                var length = pathMeasure.Length;
                if (length > 0)
                {
                    
                }
                
                if (startValue == 1f && endValue == 0f)
                {
                    LottieLog.EndSection("applyTrimPathIfNeeded");
                    return;
                }

                if (length < 1f || Math.Abs(endValue - startValue - 1) < .01)
                {
                    LottieLog.EndSection("applyTrimPathIfNeeded");
                    return;
                }

                var start = length * startValue;
                var end = length * endValue;
                var newStart = Math.Min(start, end);
                var newEnd = Math.Max(start, end);

                var offset = offsetValue * length;
                newStart += offset;
                newEnd += offset;

                // If the trim path has rotated around the path, we need to shift it back.
                if (newStart >= length && newEnd >= length)
                {
                    newStart = MiscUtils.FloorMod(newStart, length);
                    newEnd = MiscUtils.FloorMod(newEnd, length);
                }

                if (newStart < 0) newStart = MiscUtils.FloorMod(newStart, length);
                if (newEnd < 0) newEnd = MiscUtils.FloorMod(newEnd, length);

                // If the start and end are equals, return an empty path.
                if (newStart == newEnd)
                {
                    path.Reset();
                    LottieLog.EndSection("applyTrimPathIfNeeded");
                    return;
                }

                if (newStart >= newEnd) newStart -= length;

                _tempPath.Reset();
                pathMeasure.GetSegment(newStart, newEnd,ref _tempPath, true);

                if (newEnd > length)
                {
                    _tempPath2.Reset();
                    pathMeasure.GetSegment(0, newEnd % length, ref _tempPath2, true);
                    _tempPath.AddPath(_tempPath2);
                }
                else if (newStart < 0)
                {
                    _tempPath2.Reset();
                    pathMeasure.GetSegment(length + newStart, length, ref _tempPath2, true);
                    _tempPath.AddPath(_tempPath2);
                }
            }

            path.Set(_tempPath);
            LottieLog.EndSection("applyTrimPathIfNeeded");
        }

        public static Color GetSolidColor(string hex)
        {
            var index = 1; // Skip '#'
            // '#AARRGGBB'
            byte a = 255;
            if (hex.Length == 9)
            {
                a = (byte) Convert.ToUInt32(hex.Substring(index, 2), 16);
                index += 2;
            }

            var r = (byte) Convert.ToUInt32(hex.Substring(index, 2), 16);
            index += 2;
            var g = (byte) Convert.ToUInt32(hex.Substring(index, 2), 16);
            index += 2;
            var b = (byte) Convert.ToUInt32(hex.Substring(index, 2), 16);
            return new Color(a, r, g, b);
        }

        public static bool IsAtLeastVersion(int major, int minor, int patch, int minMajor, int minMinor, int minPatch)
        {
            if (major < minMajor) return false;
            if (major > minMajor) return true;

            if (minor < minMinor) return false;
            if (minor > minMinor) return true;

            return patch >= minPatch;
        }

        internal static int HashFor(double a, double  b, double  c, double  d)
        {
            var result = 17;
            if (a != 0) result = (int) (31 * result * a);
            if (b != 0) result = (int) (31 * result * b);
            if (c != 0) result = (int) (31 * result * c);
            if (d != 0) result = (int) (31 * result * d);
            return result;
        }
    }
}