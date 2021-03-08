using System;
using System.Numerics;
using Avalonia;
using Avalonia.Media; 

namespace LottieSharp.Animation.Content
{
    internal class LinearGradient : Gradient, IDisposable
    {
        private readonly float _x0;
        private readonly float _y0;
        private readonly float _x1;
        private readonly float _y1;
        private readonly GradientStops _canvasGradientStopCollection;
        private LinearGradientBrush _canvasLinearGradientBrush;

        public LinearGradient(float x0, float y0, float x1, float y1, Color[] colors, float[] positions)
        {
            _x0 = x0;
            _y0 = y0;
            _x1 = x1;
            _y1 = y1;
            _canvasGradientStopCollection = new ();
            for (var i = 0; i < colors.Length; i++)
            {
                _canvasGradientStopCollection.Add(new GradientStop
                {
                    Color = colors[i],
                    Offset = positions[i]
                });
            }
        }

        public override IBrush GetBrush(byte alpha)
        {
            if (_canvasLinearGradientBrush == null)
            {
                var startPoint = new Vector2(_x0, _y0);
                var endPoint = new Vector2(_x1, _y1);

                startPoint = LocalMatrix.Transform(startPoint);
                endPoint = LocalMatrix.Transform(endPoint);

                _canvasLinearGradientBrush = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(startPoint.X, startPoint.Y, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(endPoint.X, endPoint.Y, RelativeUnit.Relative),
                    GradientStops = _canvasGradientStopCollection
                };

            }

            _canvasLinearGradientBrush.Opacity = alpha / 255f;

            return _canvasLinearGradientBrush.ToImmutable();
        }

        private void Dispose(bool disposing)
        {
            if (_canvasLinearGradientBrush != null)
            {
                // _canvasLinearGradientBrush.Dispose();
                _canvasLinearGradientBrush = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~LinearGradient()
        {
            Dispose(false);
        }
    }
}