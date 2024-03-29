﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Avalonia.Media;
using Avalonia.Platform;

namespace Avalonia.Lottie
{
    public static class PathHelpers
    {
        
        private static T GetService<T>(this Avalonia.IAvaloniaDependencyResolver resolver) => (T) resolver.GetService(typeof (T));

        public static Avalonia.Platform.IPlatformRenderInterface Factory => Avalonia.AvaloniaLocator.Current.GetService<Avalonia.Platform.IPlatformRenderInterface>();

    }
    public class Path
    {
        
        public Path()
        {
            Contours = new List<IContour>();
            FillType = PathFillType.Winding;
        }

        public PathFillType FillType { get; set; }

        public List<IContour> Contours { get; }

        public void Set(Path path)
        {
            Contours.Clear();
            Contours.AddRange(path.Contours.Select(p => p.Copy()));
            FillType = path.FillType;
        }

        public void Transform(Matrix matrix)
        {
            for (var j = 0; j < Contours.Count; j++) Contours[j].Transform(matrix);
        }

        public IStreamGeometryImpl GetGeometry()
        {
            var v = FillRule.EvenOdd;

            switch (FillType)
            {
                case PathFillType.EvenOdd:
                    v = FillRule.EvenOdd;
                    break;
                case PathFillType.Winding:
                case PathFillType.InverseWinding:
                    v = FillRule.NonZero;
                    break;
            }

            //    FillRule = path.FillType == PathFillType.EvenOdd ? FillRule.EvenOdd : FillRule.Nonzero,
            var geometry = PathHelpers.Factory.CreateStreamGeometry();

            using (var canvasPathBuilder = geometry.Open())
            {
                canvasPathBuilder.SetFillRule(v);

                var closed = true;

                for (var i = 0; i < Contours.Count; i++) Contours[i].AddPathSegment(canvasPathBuilder, ref closed);

                if (!closed)
                    canvasPathBuilder.EndFigure(false);
            }


            return geometry;
        }

        public void ComputeBounds(ref Rect rect)
        {
            if (Contours.Count == 0)
            {
                RectExt.Set(ref rect, 0, 0, 0, 0);
                return;
            }

            //TODO: OID: Check is it ok or not
            var bound = GetGeometry().Bounds;
            rect = new Rect(bound.Left, bound.Top, bound.Right - bound.Left, bound.Bottom - bound.Top);
        }

        public void AddPath(Path path, Matrix matrix)
        {
            var pathCopy = new Path();
            pathCopy.Set(path);
            pathCopy.Transform(matrix);
            Contours.AddRange(pathCopy.Contours);
        }

        public void AddPath(Path path)
        {
            Contours.AddRange(path.Contours.Select(p => p.Copy()).ToList());
        }

        public void Reset()
        {
            Contours.Clear();
        }

        public void MoveTo(double x, double  y)
        {
            Contours.Add(new MoveToContour(x, y));
        }

        public void CubicTo(double x1, double  y1, double  x2, double  y2, double  x3, double  y3)
        {
            var bezier = new BezierContour(
                new Vector((float)x1, (float)y1),
                new Vector((float)x2, (float)y2),
                new Vector((float)x3, (float)y3)
            );
            Contours.Add(bezier);
        }

        public void LineTo(double x, double  y)
        {
            var newLine = new LineContour(x, y);
            Contours.Add(newLine);
        }

        public void Offset(double dx, double  dy)
        {
            for (var i = 0; i < Contours.Count; i++) Contours[i].Offset(dx, dy);
        }

        public void Close()
        {
            Contours.Add(new CloseContour());
        }

        /*
         Set this path to the result of applying the Op to the two specified paths. The resulting path will be constructed from non-overlapping contours. The curve order is reduced where possible so that cubics may be turned into quadratics, and quadratics maybe turned into lines.
          Path1: The first operand (for difference, the minuend)
          Path2: The second operand (for difference, the subtrahend)
        */
        public void Op(Path path1, Path path2, CanvasGeometryCombine op)
        {
            // TODO
        }

        public void ArcTo(double x, double  y, Rect rect, double  startAngle, double  sweepAngle)
        {
            var newArc = new ArcContour(new Vector((float)x, (float)y), rect, startAngle, sweepAngle);
            Contours.Add(newArc);
        }

        public double [] Approximate(double precision)
        {
            var pathIteratorFactory = new CachedPathIteratorFactory(new FullPathIterator(this));
            var pathIterator = pathIteratorFactory.Iterator();
            var points = new double [8];
            var segmentPoints = new List<Vector>();
            var lengths = new List<float>();
            var errorSquared = precision * precision;
            while (!pathIterator.Done)
            {
                var type = pathIterator.CurrentSegment(points);
                switch (type)
                {
                    case PathIterator.ContourType.MoveTo:
                        AddMove(segmentPoints, lengths, points);
                        break;
                    case PathIterator.ContourType.Close:
                        AddLine(segmentPoints, lengths, points);
                        break;
                    case PathIterator.ContourType.Line:
                        AddLine(segmentPoints, lengths, points.Skip(2).ToArray());
                        break;
                    case PathIterator.ContourType.Arc:
                        AddBezier(points, QuadraticBezierCalculation, segmentPoints, lengths, errorSquared, false);
                        break;
                    case PathIterator.ContourType.Bezier:
                        AddBezier(points, CubicBezierCalculation, segmentPoints, lengths, errorSquared, true);
                        break;
                }

                pathIterator.Next();
            }

            if (!segmentPoints.Any())
            {
                var numVerbs = Contours.Count;
                if (numVerbs == 1)
                    AddMove(segmentPoints, lengths, Contours[0].Points);
                else
                    // Invalid or empty path. Fall back to point(0,0)
                    AddMove(segmentPoints, lengths, new[] {0.0, 0.0});
            }

            var totalLength = lengths.Last();
            if (totalLength == 0)
            {
                // Lone Move instructions should still be able to animate at the same value.
                segmentPoints.Add(segmentPoints.Last());
                lengths.Add(1);
                totalLength = 1;
            }

            var numPoints = segmentPoints.Count;
            var approximationArraySize = numPoints * 3;

            var approximation = new double [approximationArraySize];

            var approximationIndex = 0;
            for (var i = 0; i < numPoints; i++)
            {
                var point = segmentPoints[i];
                approximation[approximationIndex++] = lengths[i] / totalLength;
                approximation[approximationIndex++] = point.X;
                approximation[approximationIndex++] = point.Y;
            }

            return approximation;
        }

        private static double  QuadraticCoordinateCalculation(double t, double  p0, double  p1, double  p2)
        {
            var oneMinusT = 1 - t;
            return oneMinusT * (oneMinusT * p0 + t * p1) + t * (oneMinusT * p1 + t * p2);
        }

        private static Vector QuadraticBezierCalculation(double t, double [] points)
        {
            var x = QuadraticCoordinateCalculation(t, points[0], points[2], points[4]);
            var y = QuadraticCoordinateCalculation(t, points[1], points[3], points[5]);
            return new Vector((float)x, (float)y);
        }

        private static double  CubicCoordinateCalculation(double t, double  p0, double  p1, double  p2, double  p3)
        {
            var oneMinusT = 1 - t;
            var oneMinusTSquared = oneMinusT * oneMinusT;
            var oneMinusTCubed = oneMinusTSquared * oneMinusT;
            var tSquared = t * t;
            var tCubed = tSquared * t;
            return oneMinusTCubed * p0 + 3 * oneMinusTSquared * t * p1
                                       + 3 * oneMinusT * tSquared * p2 + tCubed * p3;
        }

        private static Vector CubicBezierCalculation(double t, double [] points)
        {
            var x = CubicCoordinateCalculation(t, points[0], points[2], points[4], points[6]);
            var y = CubicCoordinateCalculation(t, points[1], points[3], points[5], points[7]);
            return new Vector((float)x, (float)y);
        }

        private static void AddMove(List<Vector> segmentPoints, List<float> lengths, double [] point)
        {
            double  length = 0;
            if (lengths.Any()) length = lengths.Last();

            segmentPoints.Add(new Vector((float)point[0], (float)point[1]));
            lengths.Add((float)length);
        }

        private static void AddLine(List<Vector> segmentPoints, List<float> lengths, double [] toPoint)
        {
            if (!segmentPoints.Any())
            {
                segmentPoints.Add(Vector.Zero);
                lengths.Add(0);
            }
            else if (segmentPoints.Last().X == toPoint[0] && segmentPoints.Last().Y == toPoint[1])
            {
                return; // Empty line
            }

            var vector2 = new Vector((float)toPoint[0], (float)toPoint[1]);
            var length = lengths.Last() + (vector2 - segmentPoints.Last()).Length;
            segmentPoints.Add(vector2);
            lengths.Add((float)length);
        }

        private static void AddBezier(double[] points, BezierCalculation bezierFunction, List<Vector> segmentPoints,
            List<float> lengths, double  errorSquared, bool doubleCheckDivision)
        {
            points[7] = points[5];
            points[6] = points[4];
            points[5] = points[3];
            points[4] = points[2];
            points[3] = points[1];
            points[2] = points[0];
            points[1] = 0;
            points[0] = 0;

            var tToPoint = new List<KeyValuePair<float, Vector>>
            {
                new(0, bezierFunction(0, points)),
                new(1, bezierFunction(1, points))
            };

            for (var i = 0; i < tToPoint.Count - 1; i++)
            {
                bool needsSubdivision;
                do
                {
                    needsSubdivision = SubdividePoints(points, bezierFunction, tToPoint[i].Key, tToPoint[i].Value,
                        tToPoint[i + 1].Key,
                        tToPoint[i + 1].Value, out var midT, out var midPoint, errorSquared);
                    if (!needsSubdivision && doubleCheckDivision)
                    {
                        needsSubdivision = SubdividePoints(points, bezierFunction, tToPoint[i].Key, tToPoint[i].Value,
                            midT,
                            midPoint, out _, out _, errorSquared);
                        if (needsSubdivision)
                            // Found an inflection point. No need to double-check.
                            doubleCheckDivision = false;
                    }

                    if (needsSubdivision) tToPoint.Insert(i + 1, new KeyValuePair<float, Vector>((float)midT, midPoint));
                } while (needsSubdivision);
            }

            // Now that each division can use linear interpolation with less than the allowed error
            foreach (var iter in tToPoint) AddLine(segmentPoints, lengths, new [] {iter.Value.X, iter.Value.Y});
        }

        private static bool SubdividePoints(double[] points, BezierCalculation bezierFunction, double  t0, Vector p0,
            double  t1, Vector p1, out double  midT, out Vector midPoint, double  errorSquared)
        {
            midT = (t1 + t0) / 2;
            var midX = (p1.X + p0.X) / 2;
            var midY = (p1.Y + p0.Y) / 2;

            midPoint = bezierFunction(midT, points);
            var xError = midPoint.X - midX;
            var yError = midPoint.Y - midY;
            var midErrorSquared = xError * xError + yError * yError;
            return midErrorSquared > errorSquared;
        }

        public interface IContour
        {
            double [] Points { get; }
            PathIterator.ContourType Type { get; }
            void Transform(Matrix matrix);
            IContour Copy();
            void AddPathSegment(IStreamGeometryContextImpl canvasPathBuilder, ref bool closed);
            void Offset(double dx, double  dy);
        }

        private class ArcContour : IContour
        {
            private readonly double  _startAngle;
            private readonly double  _sweepAngle;
            private double  _a;
            private double  _b;
            private Vector _endPoint;
            private readonly Rect _rect;
            private Vector _startPoint;

            public ArcContour(Vector startPoint, Rect rect, double  startAngle, double  sweepAngle)
            {
                _startPoint = startPoint;
                _rect = rect;
                _a =  (rect.Width / 2);
                _b =  (rect.Height / 2);
                _startAngle = startAngle;
                _sweepAngle = sweepAngle;

                _endPoint = GetPointAtAngle(startAngle + sweepAngle);
            }

            public void Transform(Matrix matrix)
            {
                _startPoint = matrix.Transform(_startPoint);
                _endPoint = matrix.Transform(_endPoint);

                var p1 = new Vector( _rect.Left,   _rect.Top);
                var p2 = new Vector(  _rect.Right,  _rect.Top);
                var p3 = new Vector( _rect.Left,  _rect.Bottom);
                var p4 = new Vector(  _rect.Right,  _rect.Bottom);

                p1 = matrix.Transform(p1);
                p2 = matrix.Transform(p2);
                p3 = matrix.Transform(p3);
                p4 = matrix.Transform(p4);

                _a = (p2 - p1).Length / 2;
                _b = (p4 - p3).Length / 2;
            }

            public IContour Copy()
            {
                return new ArcContour(_startPoint, _rect, _startAngle, _sweepAngle);
            }

            public double [] Points => new[] {_startPoint.X, _startPoint.Y, _endPoint.X, _endPoint.Y};

            public PathIterator.ContourType Type => PathIterator.ContourType.Arc;

            public void AddPathSegment(IStreamGeometryContextImpl canvasPathBuilder, ref bool closed)
            {
                // canvasPathBuilder.AddArc(new ArcSegment
                // {
                //     Point = _endPoint,
                //     RotationAngle =  MathExt.ToRadians(_sweepAngle),
                //     SweepDirection = SweepDirection.Clockwise,
                //     ArcSize = ArcSize.Small,
                //     Size = new Size2F(_a, _b)
                // });
                //

                canvasPathBuilder.ArcTo(new Point(_endPoint.X, _endPoint.Y), new Size(_a, _b),
                    MathExt.ToRadians(_sweepAngle), false, SweepDirection.Clockwise);

                closed = false;
            }

            public void Offset(double dx, double  dy)
            {

                var dV = new Vector(dx, dy);
                _startPoint += dV;
                _endPoint += dV;
            }

            private Vector GetPointAtAngle(double t)
            {
                var u = Math.Tan(MathExt.ToRadians(t) / 2);

                var u2 = u * u;

                var x = _a * (1 - u2) / (u2 + 1);
                var y = 2 * _b * u / (u2 + 1);

                return new Vector( (float) (_rect.Left + _a + x),  (float) (_rect.Top + _b + y));
            }
        }

        internal class BezierContour : IContour
        {
            private Vector _control1;
            private Vector _control2;
            private Vector _vertex;

            public BezierContour(Vector control1, Vector control2, Vector vertex)
            {
                _control1 = control1;
                _control2 = control2;
                _vertex = vertex;
            }

            public void Transform(Matrix matrix)
            {
                _control1 = matrix.Transform(_control1);
                _control2 = matrix.Transform(_control2);
                _vertex = matrix.Transform(_vertex);
            }

            public IContour Copy()
            {
                return new BezierContour(_control1, _control2, _vertex);
            }

            public double [] Points => new[] {_control1.X, _control1.Y, _control2.X, _control2.Y, _vertex.X, _vertex.Y};

            public PathIterator.ContourType Type => PathIterator.ContourType.Bezier;

            public void AddPathSegment(IStreamGeometryContextImpl canvasPathBuilder, ref bool closed)
            {
                canvasPathBuilder.CubicBezierTo(new Point(_control1.X, _control1.Y),
                    new Point(_control2.X, _control2.Y),
                    new Point(_vertex.X, _vertex.Y)
                );

                closed = false;
            }

            public void Offset(double dx, double  dy)
            {
                var dV = new Vector(dx, dy);
                _control1 += dV;
                _control2 += dV;
                _vertex += dV;
             }

            internal static double BezLength(double c0X, double  c0Y, double  c1X, double  c1Y, double  c2X, double  c2Y,
                double  c3X, double  c3Y)
            {
                const double steps = 1000d; // TODO: improve

                var length = 0d;
                double  prevPtX = 0;
                double  prevPtY = 0;

                for (var i = 0d; i < steps; i++)
                {
                    var pt = GetPointAtT(c0X, c0Y, c1X, c1Y, c2X, c2Y, c3X, c3Y, i / steps);

                    if (i > 0)
                    {
                        var x = pt.X - prevPtX;
                        var y = pt.Y - prevPtY;
                        length = length + Math.Sqrt(x * x + y * y);
                    }

                    prevPtX = pt.X;
                    prevPtY = pt.Y;
                }

                return length;
            }

            private static Vector GetPointAtT(double c0X, double  c0Y, double  c1X, double  c1Y, double  c2X, double  c2Y,
                double  c3X, double  c3Y, double t)
            {
                var t1 = 1d - t;

                if (t1 < 5e-6)
                {
                    t = 1.0;
                    t1 = 0.0;
                }

                var t13 = t1 * t1 * t1;
                var t13A = 3 * t * (t1 * t1);
                var t13B = 3 * t * t * t1;
                var t13C = t * t * t;

                var ptX =  (c0X * t13 + t13A * c1X + t13B * c2X + t13C * c3X);
                var ptY =  (c0Y * t13 + t13A * c1Y + t13B * c2Y + t13C * c3Y);

                return new Vector((float) ptX,(float)  ptY);
            }
        }

        private class LineContour : IContour
        {
            public LineContour(double x, double  y)
            {
                Points[0] = x;
                Points[1] = y;
            }

            public void Transform(Matrix matrix)
            {
                var p = new Vector((float) Points[0], (float) Points[1]);

                p = matrix.Transform(p);

                Points[0] = p.X;
                Points[1] = p.Y;
            }

            public IContour Copy()
            {
                return new LineContour(Points[0], Points[1]);
            }

            public double [] Points { get; } = new double [2];

            public PathIterator.ContourType Type => PathIterator.ContourType.Line;

            public void AddPathSegment(IStreamGeometryContextImpl canvasPathBuilder, ref bool closed)
            {
                canvasPathBuilder.LineTo(new Point(Points[0], Points[1]));

                closed = false;
            }

            public void Offset(double dx, double  dy)
            {
                Points[0] += dx;
                Points[1] += dy;
            }
        }

        private class MoveToContour : IContour
        {
            public MoveToContour(double x, double  y)
            {
                Points[0] = x;
                Points[1] = y;
            }

            public double [] Points { get; } = new double [2];

            public PathIterator.ContourType Type => PathIterator.ContourType.MoveTo;

            public IContour Copy()
            {
                return new MoveToContour(Points[0], Points[1]);
            }

            public void AddPathSegment(IStreamGeometryContextImpl canvasPathBuilder, ref bool closed)
            {
                if (!closed)
                    canvasPathBuilder.EndFigure(false);
                else
                    closed = false;

                canvasPathBuilder.BeginFigure(new Point(Points[0], Points[1]), true);
            }

            public void Offset(double dx, double  dy)
            {
                Points[0] += dx;
                Points[1] += dy;
            }

            public void Transform(Matrix matrix)
            {
                var p = new Vector((float) Points[0], (float) Points[1]);

                p = matrix.Transform(p);

                Points[0] = p.X;
                Points[1] = p.Y;
            }
        }

        private class CloseContour : IContour
        {
            public double [] Points => new double [0];

            public PathIterator.ContourType Type => PathIterator.ContourType.Close;

            public IContour Copy()
            {
                return new CloseContour();
            }

            public void AddPathSegment(IStreamGeometryContextImpl canvasPathBuilder, ref bool closed)
            {
                if (!closed)
                {
                    canvasPathBuilder.EndFigure(true);
                    closed = true;
                }
            }

            public void Offset(double dx, double  dy)
            {
            }

            public void Transform(Matrix matrix)
            {
            }
        }

        private delegate Vector BezierCalculation(double t, double [] points);
    }

    public enum PathFillType
    {
        EvenOdd,
        InverseWinding,
        Winding
    }

    public enum CanvasGeometryCombine
    {
        //
        // Summary:
        //     The result geometry contains the set of all areas from either of the source geometries.
        Union = 0,

        //
        // Summary:
        //     The result geometry contains just the areas where the source geometries overlap.
        Intersect = 1,

        //
        // Summary:
        //     The result geometry contains the areas from both the source geometries, except
        //     for any parts where they overlap.
        Xor = 2,

        //
        // Summary:
        //     The result geometry contains any area that is in the first source geometry- but
        //     excludes any area belonging to the second geometry.
        Exclude = 3
    }
}