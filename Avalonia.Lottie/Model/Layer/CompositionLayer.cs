﻿using System;
using System.Collections.Generic;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Model.Layer
{
    internal class CompositionLayer : BaseLayer
    {
        private readonly List<BaseLayer> _layers = new();
        private bool? _hasMasks;

        private bool? _hasMatte;
        private Rect _newClipRect;
        private IBaseKeyframeAnimation<float?, float?> _timeRemapping;

        internal CompositionLayer(Lottie lottie, Layer layerModel, List<Layer> layerModels,
            LottieComposition composition) : base(lottie, layerModel)
        {
            var timeRemapping = layerModel.TimeRemapping;
            if (timeRemapping != null)
            {
                _timeRemapping = timeRemapping.CreateAnimation();
                AddAnimation(_timeRemapping);
                _timeRemapping.ValueChanged += OnValueChanged;
            }
            else
            {
                _timeRemapping = null;
            }

            var layerMap = new Dictionary<long, BaseLayer>(composition.Layers.Count);

            BaseLayer mattedLayer = null;
            for (var i = layerModels.Count - 1; i >= 0; i--)
            {
                var lm = layerModels[i];
                var layer = ForModel(lm, lottie, composition);
                if (layer == null) continue;
                layerMap[layer.LayerModel.Id] = layer;
                if (mattedLayer != null)
                {
                    mattedLayer.MatteLayer = layer;
                    mattedLayer = null;
                }
                else
                {
                    _layers.Insert(0, layer);
                    switch (lm.GetMatteType())
                    {
                        case Layer.MatteType.Add:
                        case Layer.MatteType.Invert:
                            mattedLayer = layer;
                            break;
                    }
                }
            }

            foreach (var layer in layerMap)
            {
                var layerView = layer.Value;
                // This shouldn't happen but it appears as if sometimes on pre-lollipop devices when 
                // compiled with d8, layerView is null sometimes. 
                // https://github.com/airbnb/lottie-android/issues/524 
                if (layerView == null) continue;
                if (layerMap.TryGetValue(layerView.LayerModel.ParentId, out var parentLayer))
                    layerView.ParentLayer = parentLayer;
            }
        }

        public override double Progress
        {
            set
            {
                base.Progress = value;

                if (_timeRemapping?.Value != null)
                {
                    var duration = Lottie.Composition.Duration;
                    var remappedTime = (long) (_timeRemapping.Value.Value * 1000);
                    value = remappedTime / duration;
                }

                if (LayerModel.TimeStretch != 0) value /= LayerModel.TimeStretch;

                value -= LayerModel.StartProgress;
                for (var i = _layers.Count - 1; i >= 0; i--) _layers[i].Progress = value;
            }
        }

        public override void DrawLayer(LottieCanvas canvas, Matrix parentMatrix, byte parentAlpha)
        {
            LottieLog.BeginSection("CompositionLayer.Draw");

            using (canvas.Save())
            {
                RectExt.Set(ref _newClipRect, 0, 0, LayerModel.PreCompWidth, LayerModel.PreCompHeight);
                parentMatrix.MapRect(ref _newClipRect);

                for (var i = _layers.Count - 1; i >= 0; i--)
                {
                    if (!_newClipRect.IsEmpty)
                    {
                        using (canvas.ClipRect(_newClipRect))
                        {
                            var layer = _layers[i];
                            layer.Draw(canvas, parentMatrix, parentAlpha);
                        }
                    }
                }
            }

            LottieLog.EndSection("CompositionLayer.Draw");
        }

        public override void GetBounds(ref Rect outBounds, Matrix parentMatrix)
        {
            base.GetBounds(ref outBounds, parentMatrix);
            RectExt.Set(ref Rect, 0, 0, 0, 0);
            for (var i = _layers.Count - 1; i >= 0; i--)
            {
                var content = _layers[i];
                content.GetBounds(ref Rect, BoundsMatrix);
                if (outBounds.IsEmpty)
                    RectExt.Set(ref outBounds, Rect);
                else
                    RectExt.Set(ref outBounds, Math.Min(outBounds.Left, Rect.Left),
                        Math.Min(outBounds.Top, Rect.Top), Math.Max(outBounds.Right, Rect.Right),
                        Math.Max(outBounds.Bottom, Rect.Bottom));
            }
        }

        internal virtual bool HasMasks()
        {
            if (_hasMasks == null)
            {
                for (var i = _layers.Count - 1; i >= 0; i--)
                {
                    var layer = _layers[i];
                    if (layer is ShapeLayer)
                    {
                        if (layer.HasMasksOnThisLayer())
                        {
                            _hasMasks = true;
                            return true;
                        }
                    }
                    else if (layer is CompositionLayer compositionLayer && compositionLayer.HasMasks())
                    {
                        _hasMasks = true;
                        return true;
                    }
                }

                _hasMasks = false;
            }

            return _hasMasks.Value;
        }

        internal virtual bool HasMatte()
        {
            if (_hasMatte == null)
            {
                if (HasMatteOnThisLayer())
                {
                    _hasMatte = true;
                    return true;
                }

                for (var i = _layers.Count - 1; i >= 0; i--)
                    if (_layers[i].HasMatteOnThisLayer())
                    {
                        _hasMatte = true;
                        return true;
                    }

                _hasMatte = false;
            }

            return _hasMatte.Value;
        }

        internal override void ResolveChildKeyPath(KeyPath keyPath, int depth, List<KeyPath> accumulator,
            KeyPath currentPartialKeyPath)
        {
            for (var i = 0; i < _layers.Count; i++)
                _layers[i].ResolveKeyPath(keyPath, depth, accumulator, currentPartialKeyPath);
        }

        public override void AddValueCallback<T>(LottieProperty property, ILottieValueCallback<T> callback)
        {
            base.AddValueCallback(property, callback);

            if (property == LottieProperty.TimeRemap)
            {
                if (callback == null)
                {
                    _timeRemapping = null;
                }
                else
                {
                    _timeRemapping =
                        new ValueCallbackKeyframeAnimation<float?, float?>((ILottieValueCallback<float?>) callback);
                    AddAnimation(_timeRemapping);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                foreach (var item in _layers)
                    item.Dispose();

                _layers.Clear();
            }
        }
    }
}