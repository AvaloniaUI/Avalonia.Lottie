using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace Avalonia.Lottie.Animation.Content
{
    internal class LinearGradient : Gradient, IDisposable
    {
        private readonly List<ImmutableGradientStop> _canvasGradientStopCollection;
        private readonly double  _x0;
        private readonly double  _x1;
        private readonly double  _y0;
        private readonly double  _y1;
        private ImmutableLinearGradientBrush _canvasLinearGradientBrush;

        public LinearGradient(double x0, double  y0, double  x1, double  y1, Color[] colors, double [] positions)
        {
            _x0 = x0;
            _y0 = y0;
            _x1 = x1;
            _y1 = y1;
            _canvasGradientStopCollection = new List<ImmutableGradientStop>(colors.Length);
            for (var i = 0; i < colors.Length; i++)
                _canvasGradientStopCollection.Add(new ImmutableGradientStop(positions[i], colors[i]));
        }

        public void Dispose()
        {
            _canvasLinearGradientBrush = null;
        }

        public override IBrush GetBrush(byte alpha)
        {
            if (_canvasLinearGradientBrush == null)
            {
                var startPoint = new Vector2((float)_x0,(float) _y0);
                var endPoint = new Vector2((float)_x1,(float) _y1);

                startPoint = LocalMatrix.Transform(startPoint);
                endPoint = LocalMatrix.Transform(endPoint);

                _canvasLinearGradientBrush = new ImmutableLinearGradientBrush(_canvasGradientStopCollection,
                    alpha / 255f,
                    startPoint: new RelativePoint(startPoint.X, startPoint.Y, RelativeUnit.Absolute),
                    endPoint: new RelativePoint(endPoint.X, endPoint.Y, RelativeUnit.Absolute));
            }

            return _canvasLinearGradientBrush;
        }
    }
}