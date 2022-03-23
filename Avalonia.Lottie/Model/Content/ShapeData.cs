using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia.Lottie.Utils;

namespace Avalonia.Lottie.Model.Content
{
    public class ShapeData
    {
        private readonly List<CubicCurveData> _curves = new();
        private bool _closed;
        private Vector _initialPoint;

        public ShapeData(Vector initialPoint, bool closed, List<CubicCurveData> curves)
        {
            _initialPoint = initialPoint;
            _closed = closed;
            _curves.AddRange(curves);
        }

        internal ShapeData()
        {
        }

        internal virtual Vector InitialPoint => _initialPoint;

        internal virtual bool Closed => _closed;

        internal virtual List<CubicCurveData> Curves => _curves;

        private void SetInitialPoint(double x, double  y)
        {
            _initialPoint  = new Vector(x, y);
         }

        internal virtual void InterpolateBetween(ShapeData shapeData1, ShapeData shapeData2, double  percentage)
        { 
            _closed = shapeData1.Closed || shapeData2.Closed;

            if (shapeData1.Curves.Count != shapeData2.Curves.Count)
                LottieLog.Warn(
                    $"Curves must have the same number of control points. Shape 1: {shapeData1.Curves.Count}\tShape 2: {shapeData2.Curves.Count}");

            if (_curves.Count == 0)
            {
                var points = Math.Min(shapeData1.Curves.Count, shapeData2.Curves.Count);
                for (var i = 0; i < points; i++) _curves.Add(new CubicCurveData());
            }

            var initialPoint1 = shapeData1.InitialPoint;
            var initialPoint2 = shapeData2.InitialPoint;

            SetInitialPoint(MiscUtils.Lerp(initialPoint1.X, initialPoint2.X, percentage),
                MiscUtils.Lerp(initialPoint1.Y, initialPoint2.Y, percentage));

            // Manage and match curves control points
            // when interpolating... 
            // I guess the lottie js player are more forgiving than C#
            // when it comes to data mismatch.
            for (var i = Math.Min(Math.Min(shapeData1.Curves.Count, shapeData2.Curves.Count), _curves.Count) - 1; i >= 0; i--)
            {
                var curve1 = shapeData1.Curves[i];
                var curve2 = shapeData2.Curves[i];

                var cp11 = curve1.ControlPoint1;
                var cp21 = curve1.ControlPoint2;
                var vertex1 = curve1.Vertex;

                var cp12 = curve2.ControlPoint1;
                var cp22 = curve2.ControlPoint2;
                var vertex2 = curve2.Vertex;

                _curves[i].SetControlPoint1(MiscUtils.Lerp(cp11.X, cp12.X, percentage),
                    MiscUtils.Lerp(cp11.Y, cp12.Y, percentage));
                _curves[i].SetControlPoint2(MiscUtils.Lerp(cp21.X, cp22.X, percentage),
                    MiscUtils.Lerp(cp21.Y, cp22.Y, percentage));
                _curves[i].SetVertex(MiscUtils.Lerp(vertex1.X, vertex2.X, percentage),
                    MiscUtils.Lerp(vertex1.Y, vertex2.Y, percentage));
            }
        }

        public override string ToString()
        {
            return "ShapeData{" + "numCurves=" + _curves.Count + "closed=" + _closed + '}';
        }
    }
}