using System.Collections.Generic;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Model;
using Avalonia.Lottie.Model.Content;
using Avalonia.Lottie.Model.Layer;
using Avalonia.Lottie.Utils;
using Avalonia.Lottie.Value;
using Avalonia.Media;

namespace Avalonia.Lottie.Animation.Content
{
    internal class FillContent : IDrawingContent, IKeyPathElementContent
    {
        private readonly IBaseKeyframeAnimation<Color?, Color?> _colorAnimation;
        private readonly BaseLayer _layer;
        private readonly Lottie _lottie;
        private readonly IBaseKeyframeAnimation<int?, int?> _opacityAnimation;
        private readonly Paint _paint = new(Paint.AntiAliasFlag);
        private readonly Path _path = new();
        private readonly List<IPathContent> _paths = new();
        private IBaseKeyframeAnimation<ColorFilter, ColorFilter> _colorFilterAnimation;

        internal FillContent(Lottie lottie, BaseLayer layer, ShapeFill fill)
        {
            _layer = layer;
            Name = fill.Name;
            _lottie = lottie;
            if (fill.Color == null || fill.Opacity == null)
            {
                _colorAnimation = null;
                _opacityAnimation = null;
                return;
            }

            _path.FillType = fill.FillType;

            _colorAnimation = fill.Color.CreateAnimation();
            _colorAnimation.ValueChanged += (sender, args) => { _lottie.InvalidateSelf(); };
            layer.AddAnimation(_colorAnimation);
            _opacityAnimation = fill.Opacity.CreateAnimation();
            _opacityAnimation.ValueChanged += (sender, args) => { _lottie.InvalidateSelf(); };
            layer.AddAnimation(_opacityAnimation);
        }

        public virtual void SetContents(List<IContent> contentsBefore, List<IContent> contentsAfter)
        {
            for (var i = 0; i < contentsAfter.Count; i++)
            {
                var content = contentsAfter[i];
                if (content is IPathContent pathContent) _paths.Add(pathContent);
            }
        }

        public virtual string Name { get; }

        public virtual void Draw(BitmapCanvas canvas, Matrix3X3 parentMatrix, byte parentAlpha)
        {
            LottieLog.BeginSection("FillContent.Draw");
            _paint.Color = _colorAnimation.Value ?? Colors.White;
            var alpha = (byte) (parentAlpha / 255f * _opacityAnimation.Value / 100f * 255);
            _paint.Alpha = alpha;

            if (_colorFilterAnimation != null) _paint.ColorFilter = _colorFilterAnimation.Value;

            _path.Reset();
            for (var i = 0; i < _paths.Count; i++) _path.AddPath(_paths[i].Path, parentMatrix);

            canvas.DrawPath(_path, _paint);

            LottieLog.EndSection("FillContent.Draw");
        }

        public virtual void GetBounds(ref Rect outBounds, Matrix3X3 parentMatrix)
        {
            _path.Reset();
            for (var i = 0; i < _paths.Count; i++) _path.AddPath(_paths[i].Path, parentMatrix);
            _path.ComputeBounds(ref outBounds);
            // Add padding to account for rounding errors.
            RectExt.Set(ref outBounds, (float) outBounds.Left - 1, (float) outBounds.Top - 1,
                (float) outBounds.Right + 1, (float) outBounds.Bottom + 1);
        }

        public void ResolveKeyPath(KeyPath keyPath, int depth, List<KeyPath> accumulator, KeyPath currentPartialKeyPath)
        {
            MiscUtils.ResolveKeyPath(keyPath, depth, accumulator, currentPartialKeyPath, this);
        }

        public void AddValueCallback<T>(LottieProperty property, ILottieValueCallback<T> callback)
        {
            if (property == LottieProperty.Color)
            {
                _colorAnimation.SetValueCallback((ILottieValueCallback<Color?>) callback);
            }
            else if (property == LottieProperty.Opacity)
            {
                _opacityAnimation.SetValueCallback((ILottieValueCallback<int?>) callback);
            }
            else if (property == LottieProperty.ColorFilter)
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
                    _colorFilterAnimation.ValueChanged += (sender, args) => { _lottie.InvalidateSelf(); };
                    _layer.AddAnimation(_colorFilterAnimation);
                }
            }
        }
    }
}