/* Unmerged change from project 'Avalonia.Lottie (netcoreapp3.0)'
Before:
using System;
After:

*/

using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace Avalonia.Lottie.Animation.Content
{
    internal class RadialGradient : Gradient, IDisposable
    {
        private readonly List<ImmutableGradientStop> _canvasGradientStopCollection;
        private readonly double  _r;
        private readonly double  _x0;
        private readonly double  _y0;
        private readonly double  _x1;
        private readonly double  _y1;
        private IBrush _canvasRadialGradientBrush;
        private double  _ha;
        private double  _hl;

        public RadialGradient(double x0, double  y0, double  x1, double  y1, Color[] colors, double [] positions, double  highlightAngle = 0, double  highlightLength = 0)
        {
            _x0 = x0;
            _y0 = y0;
            _x1 = x1;
            _y1 = y1;
            _ha = highlightAngle;
            _hl = highlightLength;
            
            _canvasGradientStopCollection = new List<ImmutableGradientStop>();
            for (var i = 0; i < colors.Length; i++)
                _canvasGradientStopCollection.Add(new ImmutableGradientStop(positions[i], colors[i]));
        }

        public void Dispose()
        {
            _canvasRadialGradientBrush = null;
        }

        public override IBrush GetBrush(byte alpha)
        {
            if (_canvasRadialGradientBrush is not null) return _canvasRadialGradientBrush;

            var startPoint = new Vector((float) _x0, (float) _y0);
            var endPoint = new Vector((float) _x1, (float) _y1);

            startPoint = LocalMatrix.Transform(startPoint);
            endPoint = LocalMatrix.Transform(endPoint);

            var x0 = startPoint.X;
            var y0 = startPoint.Y;
            var x1 = endPoint.X;
            var y1 = endPoint.Y;
            
            var r =  Math.Sqrt(Math.Pow(x1 - x0, 2) + Math.Pow(y1 - y0, 2));
            var ang = Math.Atan2(x1 - x0, y1 - y0);

            var percent = _hl;

            percent = percent switch
            {
                >= 1 => 0.99f,
                <= -1 => -0.99f,
                _ => percent
            };

            var dist = r * percent;
            
            var fx = Math.Cos(ang + _ha) * dist + x0;
            var fy = Math.Sin(ang + _ha) * dist + y0;
            
            _canvasRadialGradientBrush = new ImmutableRadialGradientBrush(
                _canvasGradientStopCollection
                , opacity: alpha / 255f
                , center: new RelativePoint(x0, y0, RelativeUnit.Absolute)
                , gradientOrigin: new RelativePoint(fx, fy, RelativeUnit.Absolute));

            return _canvasRadialGradientBrush;
        }
    }
}