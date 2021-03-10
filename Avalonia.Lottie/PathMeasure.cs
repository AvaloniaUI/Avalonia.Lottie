using System;
using System.Numerics;
using Avalonia.Media;
using SkiaSharp;

namespace Avalonia.Lottie
{
    internal class PathMeasure : IDisposable
    {
        private Geometry _geometry;
        private SKPathMeasure _internalSKPathMeasure;
        private CachedPathIteratorFactory _originalPathIterator;
        private Path _path;

        public PathMeasure(Path path)
        {
            SetPath(path);
        }

        public float Length { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }

        public void SetPath(Path path)
        {
            // TODO: HACK HACK HACK
            _originalPathIterator = new CachedPathIteratorFactory(new FullPathIterator(path));
            _path = path;
            _geometry = _path.GetGeometry();
            var k = _geometry.PlatformImpl;
            var propertyInfo = k.GetType().GetProperty("EffectivePath");
            var internalSKPath = propertyInfo.GetValue(k, null) as SKPath;

            if (internalSKPath != null) _internalSKPathMeasure = new SKPathMeasure(internalSKPath);

            Length = _internalSKPathMeasure.Length;
        }

        public Vector2 GetPosTan(float distance)
        {
            if (distance < 0)
                distance = 0;

            var length = Length;
            if (distance > length)
                distance = length;

            // RawVector2 vectOutput;
            // var vect2 = _geometry.ComputePointAtLength(distance, out vectOutput);

            var h = _internalSKPathMeasure.GetPositionAndTangent(distance, out var position, out var tangent);

            if (h) return new Vector2(position.X, position.Y);

            return Vector2.One;
        }

        public bool GetSegment(float startD, float stopD, Path dst, bool startWithMoveTo)
        {
            var k = dst.GetGeometry().PlatformImpl;
            var propertyInfo = k.GetType().GetProperty("EffectivePath");
            var internalSKPath = propertyInfo.GetValue(k, null) as SKPath;

            if (internalSKPath != null) _internalSKPathMeasure = new SKPathMeasure(internalSKPath);

            return _internalSKPathMeasure.GetSegment(startD, stopD, internalSKPath, startWithMoveTo);
            //
            // var length = Length;
            //
            // if (startD < 0)
            // {
            //     startD = 0;
            // }
            //
            // if (stopD > length)
            // {
            //     stopD = length;
            // }
            //
            // if (startD >= stopD)
            // {
            //     return false;
            // }
            //
            // var iterator = _originalPathIterator.Iterator();
            //
            // var accLength = startD;
            // var isZeroLength = true;
            //
            // var points = new float[6];
            //
            // iterator.JumpToSegment(accLength);
            //
            // while (!iterator.Done && stopD - accLength > 0.1f)
            // {
            //     var type = iterator.CurrentSegment(points, stopD - accLength);
            //
            //     if (accLength - iterator.CurrentSegmentLength <= stopD)
            //     {
            //         if (startWithMoveTo)
            //         {
            //             startWithMoveTo = false;
            //
            //             if (type != PathIterator.ContourType.MoveTo == false)
            //             {
            //                 var lastPoint = new float[2];
            //                 iterator.GetCurrentSegmentEnd(lastPoint);
            //                 dst.MoveTo(lastPoint[0], lastPoint[1]);
            //             }
            //         }
            //
            //         isZeroLength = isZeroLength && iterator.CurrentSegmentLength > 0;
            //         switch (type)
            //         {
            //             case PathIterator.ContourType.MoveTo:
            //                 dst.MoveTo(points[0], points[1]);
            //                 break;
            //             case PathIterator.ContourType.Line:
            //                 dst.LineTo(points[0], points[1]);
            //                 break;
            //             case PathIterator.ContourType.Close:
            //                 dst.Close();
            //                 break;
            //             case PathIterator.ContourType.Bezier:
            //             case PathIterator.ContourType.Arc:
            //                 dst.CubicTo(points[0], points[1],
            //                     points[2], points[3],
            //                     points[4], points[5]);
            //                 break;
            //         }
            //     }
            //
            //     accLength += iterator.CurrentSegmentLength;
            //     iterator.Next();
            // }
            //
            // return !isZeroLength;
        }

        internal void Dispose(bool disposing)
        {
            if (_geometry != null)
            {
                // try
                // {
                //     if (disposing)
                //     {
                //         _geometry.Dispose();
                //     }
                //     else
                //     {
                //         if (System.Runtime.InteropServices.Marshal.IsComObject(_geometry))
                //         {
                //             System.Runtime.InteropServices.Marshal.ReleaseComObject(_geometry);
                //         }
                //     }
                // }
                // catch (Exception)
                // {
                //     // Ignore, but should not happen
                // }
                // finally
                // {
                _internalSKPathMeasure.Dispose();
                _geometry = null;
                // }
            }
        }

        // ~PathMeasure()
        // {
        //     Dispose(false);
        // }
    }
}