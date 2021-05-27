using System;
using System.Numerics;
using Avalonia.Media;
using Avalonia.Platform;

namespace Avalonia.Lottie
{
    internal class PathMeasure : IDisposable
    {
        private CachedPathIteratorFactory _originalPathIterator;
        private Path _path;
        private IStreamGeometryImpl _geometry;

        public PathMeasure(Path path)
        {
            SetPath(path);
        }

        public void SetPath(Path path)
        {
            _path = path;
            _geometry = null;
            _geometry = _path.GetGeometry();
            _originalPathIterator = new CachedPathIteratorFactory(new FullPathIterator(path));
            Length =  _geometry.ContourLength;
            if (Length > 0)
            {
                
            }
        }

        public double  Length { get; private set; }

        public Vector GetPosTan(double distance)
        {
            if (_geometry.TryGetPointAndTangentAtDistance(distance, out var vect2, out _))
            {
                return new Vector((float)  vect2.X,  (float) vect2.Y);
            }
            return Vector.Zero;
        }

        public bool GetSegment(double startD, double  stopD, ref Path dst, bool startWithMoveTo)
        {
            var length = Length;
            
            if (startD < 0)
            {
                startD = 0;
            }
            
            if (stopD > length)
            {
                stopD = length;
            }
            
            if (startD >= stopD)
            {
                return false;
            }
            
            var iterator = _originalPathIterator.Iterator();
            
            var accLength = startD;
            var isZeroLength = true;
            
            var points = new double [6];
            
            iterator.JumpToSegment(accLength);
            
            while (!iterator.Done && stopD - accLength > 0.1f)
            {
                var type = iterator.CurrentSegment(points, stopD - accLength);
            
                if (accLength - iterator.CurrentSegmentLength <= stopD)
                {
                    if (startWithMoveTo)
                    {
                        startWithMoveTo = false;
            
                        if (type != PathIterator.ContourType.MoveTo == false)
                        {
                            var lastPoint = new double [2];
                            iterator.GetCurrentSegmentEnd(lastPoint);
                            dst.MoveTo(lastPoint[0], lastPoint[1]);
                        }
                    }
            
                    isZeroLength = isZeroLength && iterator.CurrentSegmentLength > 0;
                    switch (type)
                    {
                        case PathIterator.ContourType.MoveTo:
                            dst.MoveTo(points[0], points[1]);
                            break;
                        case PathIterator.ContourType.Line:
                            dst.LineTo(points[0], points[1]);
                            break;
                        case PathIterator.ContourType.Close:
                            dst.Close();
                            break;
                        case PathIterator.ContourType.Bezier:
                        case PathIterator.ContourType.Arc:
                            dst.CubicTo(points[0], points[1],
                                points[2], points[3],
                                points[4], points[5]);
                            break;
                    }
                }
            
                accLength += iterator.CurrentSegmentLength;
                iterator.Next();
            }
            
            return !isZeroLength;
        }

        internal void Dispose(bool disposing)
        {
            if (_geometry != null)
            {

            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PathMeasure()
        {
            Dispose(false);
        }
    }
}