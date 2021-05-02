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
        private readonly float _r;
        private readonly float _x0;
        private readonly float _y0;
        private readonly float _fx;
        private readonly float _fy;
        private IBrush _canvasRadialGradientBrush;

        public RadialGradient(float x0, float y0, float fx, float fy, float r, Color[] colors, float[] positions)
        {
            _x0 = x0;
            _y0 = y0;
            _r = r;
            _fx = fx;
            _fy = fy;
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
            
            var center = new Vector2(_x0,_y0);
            var focal = new Vector2(_fx, _fy);
            
            center = LocalMatrix.Transform(center);
            focal = LocalMatrix.Transform(focal);
            
            _canvasRadialGradientBrush = new ImmutableRadialGradientBrush(
                _canvasGradientStopCollection
                , center: new RelativePoint(center.X, center.Y, RelativeUnit.Absolute)
                , gradientOrigin: new RelativePoint(focal.X, focal.Y, RelativeUnit.Absolute));

            return _canvasRadialGradientBrush;
        }
    }
}