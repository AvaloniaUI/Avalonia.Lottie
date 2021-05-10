/* Unmerged change from project 'Avalonia.Lottie (netcoreapp3.0)'
Before:
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Model.Content;
After:

using System;
*/

using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Model.Content;
using Avalonia.Lottie.Model.Layer;

namespace Avalonia.Lottie.Animation.Content
{
    public class GradientStrokeContent : BaseStrokeContent
    {
        /// <summary>
        ///     Cache the gradients such that it runs at 30fps.
        /// </summary>
        private const int CacheStepsMs = 32;

        private readonly int _cacheSteps;
        private readonly IBaseKeyframeAnimation<GradientColor, GradientColor> _colorAnimation;
        private readonly IBaseKeyframeAnimation<Vector2?, Vector2?> _endPointAnimation;

        private readonly Dictionary<long, LinearGradient> _linearGradientCache = new();
        private readonly Dictionary<long, RadialGradient> _radialGradientCache = new();
        private readonly IBaseKeyframeAnimation<Vector2?, Vector2?> _startPointAnimation;

        private readonly GradientType _type;
        private Rect _boundsRect;

        internal GradientStrokeContent(Lottie lottie, BaseLayer layer, GradientStroke stroke)
            : base(lottie, layer, ShapeStroke.LineCapTypeToPaintCap(stroke.CapType),
                ShapeStroke.LineJoinTypeToPaintLineJoin(stroke.JoinType), stroke.MiterLimit, stroke.Opacity,
                stroke.Width, stroke.LineDashPattern, stroke.DashOffset)
        {
            Name = stroke.Name;
            _type = stroke.GradientType;
            _cacheSteps = (int) (lottie.Composition.Duration / CacheStepsMs);

            _colorAnimation = stroke.GradientColor.CreateAnimation();
            _colorAnimation.ValueChanged += OnValueChanged;
            layer.AddAnimation(_colorAnimation);

            _startPointAnimation = stroke.StartPoint.CreateAnimation();
            _startPointAnimation.ValueChanged += OnValueChanged;
            layer.AddAnimation(_startPointAnimation);

            _endPointAnimation = stroke.EndPoint.CreateAnimation();
            _endPointAnimation.ValueChanged += OnValueChanged;
            layer.AddAnimation(_endPointAnimation);
        }

        public override string Name { get; }

        private LinearGradient LinearGradient
        {
            get
            {
                var gradientHash = GradientHash;
                if (_linearGradientCache.TryGetValue(gradientHash, out var gradient)) return gradient;
                var startPoint = _startPointAnimation.Value;
                var endPoint = _endPointAnimation.Value;
                var gradientColor = _colorAnimation.Value;
                var colors = gradientColor.Colors;
                var positions = gradientColor.Positions;
                var x0 = (int) (_boundsRect.Left + _boundsRect.Width / 2 + startPoint.Value.X);
                var y0 = (int) (_boundsRect.Top + _boundsRect.Height / 2 + startPoint.Value.Y);
                var x1 = (int) (_boundsRect.Left + _boundsRect.Width / 2 + endPoint.Value.X);
                var y1 = (int) (_boundsRect.Top + _boundsRect.Height / 2 + endPoint.Value.Y);
                gradient = new LinearGradient(x0, y0, x1, y1, colors, positions);
                _linearGradientCache.Add(gradientHash, gradient);
                return gradient;
            }
        }

        private RadialGradient RadialGradient
        {
            get
            {
                var gradientHash = GradientHash;
                if (_radialGradientCache.TryGetValue(gradientHash, out var gradient)) return gradient;
                var startPoint = _startPointAnimation.Value;
                var endPoint = _endPointAnimation.Value;
                var gradientColor = _colorAnimation.Value;
                var colors = gradientColor.Colors;
                var positions = gradientColor.Positions;
                var x0 = (int) (_boundsRect.Left + _boundsRect.Width / 2 + startPoint.Value.X);
                var y0 = (int) (_boundsRect.Top + _boundsRect.Height / 2 + startPoint.Value.Y);
                var x1 = (int) (_boundsRect.Left + _boundsRect.Width / 2 + endPoint.Value.X);
                var y1 = (int) (_boundsRect.Top + _boundsRect.Height / 2 + endPoint.Value.Y);
                gradient = new RadialGradient(x0, y0, x1, y1, colors, positions);
                _radialGradientCache.Add(gradientHash, gradient);
                return gradient;
            }
        }

        private int GradientHash
        {
            get
            {
                var startPointProgress = (int) Math.Round(_startPointAnimation.Progress * _cacheSteps);
                var endPointProgress = (int) Math.Round(_endPointAnimation.Progress * _cacheSteps);
                var colorProgress = (int) Math.Round(_colorAnimation.Progress * _cacheSteps);
                var hash = 17;
                if (startPointProgress != 0)
                    hash = hash * 31 * startPointProgress;
                if (endPointProgress != 0)
                    hash = hash * 31 * endPointProgress;
                if (colorProgress != 0)
                    hash = hash * 31 * colorProgress;
                return hash;
            }
        }

        public override void Draw(BitmapCanvas canvas, Matrix3X3 parentMatrix, byte parentAlpha)
        {
            GetBounds(ref _boundsRect, parentMatrix);
            if (_type == GradientType.Linear)
                Paint.Shader = LinearGradient;
            else
                Paint.Shader = RadialGradient;

            base.Draw(canvas, parentMatrix, parentAlpha);
        }
    }
}