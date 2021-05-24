using System;
using System.Collections.Generic;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Model;
using Avalonia.Lottie.Model.Content;
using Avalonia.Lottie.Model.Layer;
using Avalonia.Lottie.Utils;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Animation.Content
{
    public class RepeaterContent : IDrawingContent, IPathContent, IGreedyContent, IKeyPathElementContent
    {
        private readonly IBaseKeyframeAnimation<float?, float?> _copies;
        private readonly BaseLayer _layer;

        private readonly Lottie _lottie;
        private readonly IBaseKeyframeAnimation<float?, float?> _offset;
        private readonly Path _path = new();
        private readonly TransformKeyframeAnimation _transform;
        private ContentGroup _contentGroup;
        private Matrix _matrix =Matrix.Identity;

        internal RepeaterContent(Lottie lottie, BaseLayer layer, Repeater repeater)
        {
            _lottie = lottie;
            _layer = layer;
            Name = repeater.Name;
            _copies = repeater.Copies.CreateAnimation();
            layer.AddAnimation(_copies);
            _copies.ValueChanged += OnValueChanged;

            _offset = repeater.Offset.CreateAnimation();
            layer.AddAnimation(_offset);
            _offset.ValueChanged += OnValueChanged;

            _transform = repeater.Transform.CreateAnimation();
            _transform.AddAnimationsToLayer(layer);
            _transform.ValueChanged += OnValueChanged;
        }

        public string Name { get; }

        public void SetContents(List<IContent> contentsBefore, List<IContent> contentsAfter)
        {
            _contentGroup.SetContents(contentsBefore, contentsAfter);
        }

        public void Draw(LottieCanvas canvas, Matrix parentMatrix, byte alpha)
        {
            var copies = _copies.Value.Value;
            var offset = _offset.Value.Value;
            var startOpacity = _transform.StartOpacity.Value.Value / 100f;
            var endOpacity = _transform.EndOpacity.Value.Value / 100f;
            for (var i = (int) copies - 1; i >= 0; i--)
            {
                _matrix = (parentMatrix);
                _matrix = MatrixExt.PreConcat(_matrix, _transform.GetMatrixForRepeater(i + offset));
                var newAlpha = alpha * MiscUtils.Lerp(startOpacity, endOpacity, i / copies);
                _contentGroup.Draw(canvas, _matrix, (byte) newAlpha);
            }
        }

        public void GetBounds(ref Rect outBounds, Matrix parentMatrix)
        {
            _contentGroup.GetBounds(ref outBounds, parentMatrix);
        }

        public void AbsorbContent(List<IContent> contentsIter)
        {
            // This check prevents a repeater from getting added twice.
            // This can happen in the following situation:
            //    RECTANGLE
            //    REPEATER 1
            //    FILL
            //    REPEATER 2
            // In this case, the expected structure would be:
            //     REPEATER 2
            //        REPEATER 1
            //            RECTANGLE
            //        FILL
            // Without this check, REPEATER 1 will try and absorb contents once it is already inside of
            // REPEATER 2.
            if (_contentGroup != null) return;
            // Fast forward the iterator until after this content.
            var index = contentsIter.Count;
            while (index > 0)
            {
                index--;
                if (contentsIter[index] == this)
                    break;
            }

            var contents = new List<IContent>();
            while (index > 0)
            {
                index--;
                contents.Add(contentsIter[index]);
                contentsIter.RemoveAt(index);
            }

            contents.Reverse();
            _contentGroup = new ContentGroup(_lottie, _layer, "Repeater", contents, null);
        }

        public void ResolveKeyPath(KeyPath keyPath, int depth, List<KeyPath> accumulator, KeyPath currentPartialKeyPath)
        {
            MiscUtils.ResolveKeyPath(keyPath, depth, accumulator, currentPartialKeyPath, this);
        }

        public void AddValueCallback<T>(LottieProperty property, ILottieValueCallback<T> callback)
        {
            if (_transform.ApplyValueCallback(property, callback)) return;

            if (property == LottieProperty.RepeaterCopies)
                _copies.SetValueCallback((ILottieValueCallback<float?>) callback);
            else if (property == LottieProperty.RepeaterOffset)
                _offset.SetValueCallback((ILottieValueCallback<float?>) callback);
        }

        public Path Path
        {
            get
            {
                var contentPath = _contentGroup.Path;
                _path.Reset();
                var copies = _copies.Value.Value;
                var offset = _offset.Value.Value;
                for (var i = (int) copies - 1; i >= 0; i--)
                {
                    _matrix = (_transform.GetMatrixForRepeater(i + offset));
                    _path.AddPath(contentPath, _matrix);
                }

                return _path;
            }
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            _lottie.InvalidateSelf();
        }
    }
}