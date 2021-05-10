/* Unmerged change from project 'Avalonia.Lottie (netcoreapp3.0)'
Before:

After:


using System;
using System.Collections.Generic;
*/

using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Model;
using Avalonia.Lottie.Model.Content;
using Avalonia.Lottie.Model.Layer;
using Avalonia.Lottie.Utils;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Animation.Content
{
    internal class GradientFillContent : IDrawingContent, IKeyPathElementContent
    {
        /// <summary>
        ///     Cache the gradients such that it runs at 30fps.
        /// </summary>
        private const int CacheStepsMs = 32;

        private readonly int _cacheSteps;
        private readonly IBaseKeyframeAnimation<GradientColor, GradientColor> _colorAnimation;
        private readonly IBaseKeyframeAnimation<Vector2?, Vector2?> _endPointAnimation;

        private readonly BaseLayer _layer;
        private readonly Dictionary<long, LinearGradient> _linearGradientCache = new();
        private readonly Lottie _lottie;
        private readonly IBaseKeyframeAnimation<int?, int?> _opacityAnimation;
        private readonly Paint _paint = new(Paint.AntiAliasFlag);

        private readonly Path _path = new();

        //private Rect _boundsRect;
        private readonly List<IPathContent> _paths = new();
        private readonly Dictionary<long, RadialGradient> _radialGradientCache = new();
        private readonly Matrix3X3 _shaderMatrix = Matrix3X3.CreateIdentity();
        private readonly IBaseKeyframeAnimation<Vector2?, Vector2?> _startPointAnimation;
        private readonly GradientType _type;
        private IBaseKeyframeAnimation<ColorFilter, ColorFilter> _colorFilterAnimation;
        private readonly IBaseKeyframeAnimation<float?, float?> _highlightAngleAnimation;
        private readonly IBaseKeyframeAnimation<float?, float?> _highlightLengthAnimation;

        internal GradientFillContent(Lottie lottie, BaseLayer layer, GradientFill fill)
        {
            _layer = layer;
            Name = fill.Name;
            _lottie = lottie;
            _type = fill.GradientType;
            _path.FillType = fill.FillType;
            _cacheSteps = (int) (lottie.Composition.Duration / CacheStepsMs);

            _colorAnimation = fill.GradientColor.CreateAnimation();
            _colorAnimation.ValueChanged += OnValueChanged;
            layer.AddAnimation(_colorAnimation);

            _opacityAnimation = fill.Opacity.CreateAnimation();
            _opacityAnimation.ValueChanged += OnValueChanged;
            layer.AddAnimation(_opacityAnimation);

            _startPointAnimation = fill.StartPoint.CreateAnimation();
            _startPointAnimation.ValueChanged += OnValueChanged;
            layer.AddAnimation(_startPointAnimation);

            _endPointAnimation = fill.EndPoint.CreateAnimation();
            _endPointAnimation.ValueChanged += OnValueChanged;
            layer.AddAnimation(_endPointAnimation);

            if (fill.HighlightAngle is not null)
            {
                _highlightAngleAnimation = fill.HighlightAngle.CreateAnimation();
                _highlightAngleAnimation.ValueChanged += OnValueChanged;
                layer.AddAnimation(_highlightAngleAnimation);
            }

            if (fill.HighlightLength is not null)
            {
                _highlightLengthAnimation = fill.HighlightLength.CreateAnimation();
                _highlightLengthAnimation.ValueChanged += OnValueChanged;
                layer.AddAnimation(_highlightLengthAnimation);
            }
        }

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
                gradient = new LinearGradient(startPoint.Value.X, startPoint.Value.Y, endPoint.Value.X,
                    endPoint.Value.Y, colors, positions);
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
                var x0 = startPoint.Value.X;
                var y0 = startPoint.Value.Y;
                var x1 = endPoint.Value.X;
                var y1 = endPoint.Value.Y;
                
                var ha = _highlightAngleAnimation.Value ?? 0;
                var hl = _highlightLengthAnimation.Value ?? 0;

                gradient = new RadialGradient(x0, y0, x1, y1, colors, positions, ha, hl);
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

        public void SetContents(List<IContent> contentsBefore, List<IContent> contentsAfter)
        {
            for (var i = 0; i < contentsAfter.Count; i++)
                if (contentsAfter[i] is IPathContent pathContent)
                    _paths.Add(pathContent);
        }

        public void Draw(BitmapCanvas canvas, Matrix3X3 parentMatrix, byte parentAlpha)
        {
            LottieLog.BeginSection("GradientFillContent.Draw");
            _path.Reset();
            for (var i = 0; i < _paths.Count; i++) _path.AddPath(_paths[i].Path, parentMatrix);

            //_path.ComputeBounds(out _boundsRect);

            Shader shader;
            if (_type == GradientType.Linear)
                shader = LinearGradient;
            else
                shader = RadialGradient;
            _shaderMatrix.Set(parentMatrix);
            shader.LocalMatrix = _shaderMatrix;
            _paint.Shader = shader;

            if (_colorFilterAnimation != null) _paint.ColorFilter = _colorFilterAnimation.Value;

            var alpha = (byte) (parentAlpha / 255f * _opacityAnimation.Value / 100f * 255);
            _paint.Alpha = alpha;

            canvas.DrawPath(_path, _paint);
            LottieLog.EndSection("GradientFillContent.Draw");
        }

        public void GetBounds(ref Rect outBounds, Matrix3X3 parentMatrix)
        {
            _path.Reset();
            for (var i = 0; i < _paths.Count; i++) _path.AddPath(_paths[i].Path, parentMatrix);

            _path.ComputeBounds(ref outBounds);
            // Add padding to account for rounding errors.
            RectExt.Set(ref outBounds, (float) outBounds.Left - 1, (float) outBounds.Top - 1,
                (float) outBounds.Right + 1, (float) outBounds.Bottom + 1);
        }

        public string Name { get; }

        public void ResolveKeyPath(KeyPath keyPath, int depth, List<KeyPath> accumulator, KeyPath currentPartialKeyPath)
        {
            MiscUtils.ResolveKeyPath(keyPath, depth, accumulator, currentPartialKeyPath, this);
        }

        public void AddValueCallback<T>(LottieProperty property, ILottieValueCallback<T> callback)
        {
            if (property == LottieProperty.ColorFilter)
            {
                if (callback == null)
                {
                    _colorFilterAnimation = null;
                }
                else
                {
                    _colorFilterAnimation =
                        new ValueCallbackKeyframeAnimation<ColorFilter, ColorFilter>(
                            (ILottieValueCallback<ColorFilter>) callback);
                    _colorFilterAnimation.ValueChanged += OnValueChanged;
                    _layer.AddAnimation(_colorFilterAnimation);
                }
            }
        }

        private void OnValueChanged(object sender, EventArgs eventArgs)
        {
            _lottie.InvalidateSelf();
        }
    }
}