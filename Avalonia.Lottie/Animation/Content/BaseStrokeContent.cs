﻿using System;
using System.Collections.Generic;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Model;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Content;
using Avalonia.Lottie.Model.Layer;
using Avalonia.Lottie.Utils;
using Avalonia.Lottie.Value;
using Avalonia.Media;

namespace Avalonia.Lottie.Animation.Content
{
    public abstract class BaseStrokeContent : IDrawingContent, IKeyPathElementContent
    {
        private readonly List<IBaseKeyframeAnimation<float?, float?>> _dashPatternAnimations;
        private readonly IBaseKeyframeAnimation<float?, float?> _dashPatternOffsetAnimation;
        private readonly double[] _dashPatternValues;
        private readonly BaseLayer _layer;
        private readonly Lottie _lottie;
        private readonly IBaseKeyframeAnimation<int?, int?> _opacityAnimation;
        private readonly Path _path = new();
        private readonly List<PathGroup> _pathGroups = new();
        private readonly Path _trimPathPath = new();

        private readonly IBaseKeyframeAnimation<float?, float?> _widthAnimation;
        internal readonly Paint Paint = new(Paint.AntiAliasFlag);
        private IBaseKeyframeAnimation<ColorFilter, ColorFilter> _colorFilterAnimation;
        private Rect _rect;

        internal BaseStrokeContent(Lottie lottie, BaseLayer layer, PenLineCap cap, PenLineJoin join,
            double  miterLimit, AnimatableIntegerValue opacity, AnimatableFloatValue width,
            List<AnimatableFloatValue> dashPattern, AnimatableFloatValue offset)
        {
            _lottie = lottie;
            _layer = layer;

            Paint.Style = Paint.PaintStyle.Stroke;
            Paint.StrokeCap = cap;
            Paint.StrokeJoin = join;
            Paint.StrokeMiter = miterLimit;

            _opacityAnimation = opacity.CreateAnimation();
            _widthAnimation = width.CreateAnimation();

            if (offset == null)
                _dashPatternOffsetAnimation = null;
            else
                _dashPatternOffsetAnimation = offset.CreateAnimation();
            _dashPatternAnimations = new List<IBaseKeyframeAnimation<float?, float?>>(dashPattern.Count);
            _dashPatternValues = new double [dashPattern.Count];

            for (var i = 0; i < dashPattern.Count; i++) _dashPatternAnimations.Add(dashPattern[i].CreateAnimation());

            layer.AddAnimation(_opacityAnimation);
            layer.AddAnimation(_widthAnimation);
            for (var i = 0; i < _dashPatternAnimations.Count; i++) layer.AddAnimation(_dashPatternAnimations[i]);
            if (_dashPatternOffsetAnimation != null) layer.AddAnimation(_dashPatternOffsetAnimation);

            _opacityAnimation.ValueChanged += OnValueChanged;
            _widthAnimation.ValueChanged += OnValueChanged;

            for (var i = 0; i < dashPattern.Count; i++) _dashPatternAnimations[i].ValueChanged += OnValueChanged;
            if (_dashPatternOffsetAnimation != null) _dashPatternOffsetAnimation.ValueChanged += OnValueChanged;
        }

        public abstract string Name { get; }

        public void SetContents(List<IContent> contentsBefore, List<IContent> contentsAfter)
        {
            TrimPathContent trimPathContentBefore = null;
            for (var i = contentsBefore.Count - 1; i >= 0; i--)
            {
                var content = contentsBefore[i];
                if (content is TrimPathContent trimPathContent &&
                    trimPathContent.Type == ShapeTrimPath.Type.Individually) trimPathContentBefore = trimPathContent;
            }

            if (trimPathContentBefore != null) trimPathContentBefore.ValueChanged += OnValueChanged;

            PathGroup currentPathGroup = null;
            for (var i = contentsAfter.Count - 1; i >= 0; i--)
            {
                var content = contentsAfter[i];
                if (content is TrimPathContent trimPathContent &&
                    trimPathContent.Type == ShapeTrimPath.Type.Individually)
                {
                    if (currentPathGroup != null) _pathGroups.Add(currentPathGroup);
                    currentPathGroup = new PathGroup(trimPathContent);
                    trimPathContent.ValueChanged += OnValueChanged;
                }
                else if (content is IPathContent)
                {
                    if (currentPathGroup == null) currentPathGroup = new PathGroup(trimPathContentBefore);
                    currentPathGroup.Paths.Add((IPathContent) content);
                }
            }

            if (currentPathGroup != null) _pathGroups.Add(currentPathGroup);
        }

        public virtual void Draw(LottieCanvas canvas, Matrix parentMatrix, byte parentAlpha)
        {
            LottieLog.BeginSection("StrokeContent.Draw");
            var alpha = (byte) (parentAlpha / 255f * _opacityAnimation.Value / 100f * 255);
            Paint.Alpha = alpha;
            Paint.StrokeWidth = _widthAnimation.Value.Value * Utils.Utils.GetScale(parentMatrix);
            if (Paint.StrokeWidth <= 0)
            {
                // Android draws a hairline stroke for 0, After Effects doesn't.
                LottieLog.EndSection("StrokeContent.Draw");
                return;
            }

            ApplyDashPatternIfNeeded(parentMatrix);

            if (_colorFilterAnimation != null) Paint.ColorFilter = _colorFilterAnimation.Value;

            for (var i = 0; i < _pathGroups.Count; i++)
            {
                var pathGroup = _pathGroups[i];

                if (pathGroup.TrimPath != null)
                {
                    ApplyTrimPath(canvas, pathGroup, parentMatrix);
                }
                else
                {
                    LottieLog.BeginSection("StrokeContent.BuildPath");
                    _path.Reset();
                    for (var j = pathGroup.Paths.Count - 1; j >= 0; j--)
                        _path.AddPath(pathGroup.Paths[j].Path, parentMatrix);
                    LottieLog.EndSection("StrokeContent.BuildPath");
                    LottieLog.BeginSection("StrokeContent.DrawPath");
                    canvas.DrawPath(_path, Paint);
                    LottieLog.EndSection("StrokeContent.DrawPath");
                }
            }

            LottieLog.EndSection("StrokeContent.Draw");
        }

        public void GetBounds(ref Rect outBounds, Matrix parentMatrix)
        {
            LottieLog.BeginSection("StrokeContent.GetBounds");
            _path.Reset();
            for (var i = 0; i < _pathGroups.Count; i++)
            {
                var pathGroup = _pathGroups[i];
                for (var j = 0; j < pathGroup.Paths.Count; j++) _path.AddPath(pathGroup.Paths[j].Path, parentMatrix);
            }

            _path.ComputeBounds(ref _rect);

            var width = _widthAnimation.Value.Value;
            RectExt.Set(ref _rect,  (_rect.Left - width / 2f),  (_rect.Top - width / 2f),
                 _rect.Right + width / 2f,  _rect.Bottom + width / 2f);
            RectExt.Set(ref outBounds, _rect);
            // Add padding to account for rounding errors.
            RectExt.Set(ref outBounds,  outBounds.Left - 1,  outBounds.Top - 1,
                 outBounds.Right + 1,  outBounds.Bottom + 1);
            LottieLog.EndSection("StrokeContent.GetBounds");
        }

        public void ResolveKeyPath(KeyPath keyPath, int depth, List<KeyPath> accumulator, KeyPath currentPartialKeyPath)
        {
            MiscUtils.ResolveKeyPath(keyPath, depth, accumulator, currentPartialKeyPath, this);
        }

        public virtual void AddValueCallback<T>(LottieProperty property, ILottieValueCallback<T> callback)
        {
            if (property == LottieProperty.Opacity)
            {
                _opacityAnimation.SetValueCallback((ILottieValueCallback<int?>) callback);
            }
            else if (property == LottieProperty.StrokeWidth)
            {
                _widthAnimation.SetValueCallback((ILottieValueCallback<float?>) callback);
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
                    _colorFilterAnimation.ValueChanged += OnValueChanged;
                    _layer.AddAnimation(_colorFilterAnimation);
                }
            }
        }

        public virtual void OnValueChanged(object sender, EventArgs eventArgs)
        {
            _lottie.InvalidateSelf();
        }

        private void ApplyTrimPath(LottieCanvas canvas, PathGroup pathGroup, Matrix parentMatrix)
        {
            LottieLog.BeginSection("StrokeContent.ApplyTrimPath");
            if (pathGroup.TrimPath == null)
            {
                LottieLog.EndSection("StrokeContent.ApplyTrimPath");
                return;
            }

            _path.Reset();
            for (var j = pathGroup.Paths.Count - 1; j >= 0; j--) _path.AddPath(pathGroup.Paths[j].Path, parentMatrix);
            double  totalLength;
            using (var pm = new PathMeasure(_path))
            {
                totalLength = pm.Length;
            }

            var offsetLength = totalLength * pathGroup.TrimPath.Offset.Value.Value / 360f;
            var startLength = totalLength * pathGroup.TrimPath.Start.Value.Value / 100f + offsetLength;
            var endLength = totalLength * pathGroup.TrimPath.End.Value.Value / 100f + offsetLength;

            double  currentLength = 0;
            for (var j = pathGroup.Paths.Count - 1; j >= 0; j--)
            {
                _trimPathPath.Set(pathGroup.Paths[j].Path);
                _trimPathPath.Transform(parentMatrix);

                double  length;
                using (var pm = new PathMeasure(_trimPathPath))
                {
                    length = pm.Length;
                }

                if (endLength > totalLength && endLength - totalLength < currentLength + length &&
                    currentLength < endLength - totalLength)
                {
                    // Draw the segment when the end is greater than the length which wraps around to the
                    // beginning.
                    double  startValue;
                    if (startLength > totalLength)
                        startValue = (startLength - totalLength) / length;
                    else
                        startValue = 0;
                    var endValue = Math.Min((endLength - totalLength) / length, 1);
                    Utils.Utils.ApplyTrimPathIfNeeded(_trimPathPath, startValue, endValue, 0);
                    canvas.DrawPath(_trimPathPath, Paint);
                }
                else
                {
                    if (currentLength + length < startLength || currentLength > endLength)
                    {
                        // Do nothing
                    }
                    else if (currentLength + length <= endLength && startLength < currentLength)
                    {
                        canvas.DrawPath(_trimPathPath, Paint);
                    }
                    else
                    {
                        double  startValue;
                        if (startLength < currentLength)
                            startValue = 0;
                        else
                            startValue = (startLength - currentLength) / length;
                        double  endValue;
                        if (endLength > currentLength + length)
                            endValue = 1f;
                        else
                            endValue = (endLength - currentLength) / length;
                        Utils.Utils.ApplyTrimPathIfNeeded(_trimPathPath, startValue, endValue, 0);
                        canvas.DrawPath(_trimPathPath, Paint);
                    }
                }

                currentLength += length;
            }

            LottieLog.EndSection("StrokeContent.ApplyTrimPath");
        }

        private void ApplyDashPatternIfNeeded(Matrix parentMatrix)
        {
            LottieLog.BeginSection("StrokeContent.ApplyDashPattern");
            if (_dashPatternAnimations.Count == 0)
            {
                LottieLog.EndSection("StrokeContent.ApplyDashPattern");
                return;
            }

            var scale = Utils.Utils.GetScale(parentMatrix);
            for (var i = 0; i < _dashPatternAnimations.Count; i++)
            {
                _dashPatternValues[i] = _dashPatternAnimations[i].Value.Value;
                // If the value of the dash pattern or gap is too small, the number of individual sections
                // approaches infinity as the value approaches 0.
                // To mitigate this, we essentially put a minimum value on the dash pattern size of 1px
                // and a minimum gap size of 0.01.
                if (i % 2 == 0)
                {
                    if (_dashPatternValues[i] < 1f) _dashPatternValues[i] = 1f;
                }
                else
                {
                    if (_dashPatternValues[i] < 0.1f) _dashPatternValues[i] = 0.1f;
                }

                _dashPatternValues[i] *= scale;
            }

            var offset = _dashPatternOffsetAnimation?.Value ?? 0f;
            Paint.PathEffect = new DashPathEffect(_dashPatternValues, offset);
            LottieLog.EndSection("StrokeContent.ApplyDashPattern");
        }

        /// <summary>
        ///     Data class to help drawing trim paths individually.
        /// </summary>
        private sealed class PathGroup
        {
            internal readonly List<IPathContent> Paths = new();
            internal readonly TrimPathContent TrimPath;

            internal PathGroup(TrimPathContent trimPath)
            {
                TrimPath = trimPath;
            }
        }
    }
}