/* Unmerged change from project 'LottieSharp (netcoreapp3.0)'
Before:
using System;
After:

*/

using System;
using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;

namespace LottieSharp.Animation.Content
{
    internal class RadialGradient : Gradient, IDisposable
    {
        private readonly float _x0;
        private readonly float _y0;
        private readonly float _r;
        private readonly GradientStops _canvasGradientStopCollection;
        private RadialGradientBrush _canvasRadialGradientBrush;

        public RadialGradient(float x0, float y0, float r, Color[] colors, float[] positions)
        {
            _x0 = x0;
            _y0 = y0;
            _r = r;
            _canvasGradientStopCollection = new();
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
            if (_canvasRadialGradientBrush == null)
            {
                var center = new Vector2(_x0, _y0);
                center = LocalMatrix.Transform(center);
                
                //
                // var properties = new RadialGradientBrushProperties
                // {
                //     RadiusX = _r,
                //     RadiusY = _r,
                //     Center = center
                // };
                //
                // var collection = new GradientStopCollection(renderTarget, _canvasGradientStopCollection, Gamma.Linear, ExtendMode.Clamp);
                // //TODO: OID: property missed, Same for Linear 
                //

                _canvasRadialGradientBrush = new RadialGradientBrush
                {
                    Radius = _r,
                    Center = new RelativePoint(center.X, center.Y, RelativeUnit.Relative),
                    GradientStops = _canvasGradientStopCollection
                };
            }

            _canvasRadialGradientBrush.Opacity = alpha / 255f;

            return _canvasRadialGradientBrush.ToImmutable();
        }

        private void Dispose(bool disposing)
        {
            if (_canvasRadialGradientBrush != null)
            {
                _canvasRadialGradientBrush = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RadialGradient()
        {
            Dispose(false);
        }
    }
}