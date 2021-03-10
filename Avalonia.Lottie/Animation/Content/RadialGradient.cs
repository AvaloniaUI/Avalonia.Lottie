/* Unmerged change from project 'Avalonia.Lottie (netcoreapp3.0)'
Before:
using System;
After:

*/

using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;

namespace Avalonia.Lottie.Animation.Content
{
    internal class RadialGradient : Gradient, IDisposable
    {
        private readonly float _x0;
        private readonly float _y0;
        private readonly float _r;
        private readonly List<ImmutableGradientStop> _canvasGradientStopCollection;
        private ImmutableRadialGradientBrush _canvasRadialGradientBrush;

        public RadialGradient(float x0, float y0, float r, Color[] colors, float[] positions)
        {
            _x0 = x0;
            _y0 = y0;
            _r = r;
            _canvasGradientStopCollection = new();
            for (var i = 0; i < colors.Length; i++)
            {
                _canvasGradientStopCollection.Add(new ImmutableGradientStop(positions[i], colors[i]));
            }
        }

        public override IBrush GetBrush(byte alpha)
        { 
            if (_canvasRadialGradientBrush == null)
            {
                var center = new Vector2(_x0, _y0);
                center = LocalMatrix.Transform(center);
                _canvasRadialGradientBrush = new ImmutableRadialGradientBrush(_canvasGradientStopCollection,
                    alpha / 255f, center: new RelativePoint(center.X, center.Y, RelativeUnit.Absolute), radius: _r);
            }

            return _canvasRadialGradientBrush;
        }
        
        public void Dispose()
        {
            _canvasRadialGradientBrush = null;
        }
    }
}