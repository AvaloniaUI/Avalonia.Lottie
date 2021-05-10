using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Model;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Content;
using Avalonia.Lottie.Model.Layer;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Animation.Content
{
    internal class ContentGroup : IDrawingContent, IPathContent, IKeyPathElement
    {
        private readonly List<IContent> _contents;
        private readonly Path _path = new();
        private readonly TransformKeyframeAnimation _transformAnimation;

        private Matrix3X3 _matrix = Matrix3X3.CreateIdentity();
        private List<IPathContent> _pathContents;
        private Rect _rect;

        internal ContentGroup(Lottie lottie, BaseLayer layer, ShapeGroup shapeGroup)
            : this(lottie, layer, shapeGroup.Name,
                ContentsFromModels(lottie, layer, shapeGroup.Items),
                FindTransform(shapeGroup.Items))
        {
        }

        internal ContentGroup(Lottie lottie, BaseLayer layer, string name, List<IContent> contents,
            AnimatableTransform transform)
        {
            Name = name;
            _contents = contents;

            if (transform != null)
            {
                _transformAnimation = transform.CreateAnimation();

                _transformAnimation.AddAnimationsToLayer(layer);
                _transformAnimation.ValueChanged += (sender, args) => { lottie.InvalidateSelf(); };
            }

            var greedyContents = new List<IGreedyContent>();
            for (var i = contents.Count - 1; i >= 0; i--)
            {
                var content = contents[i];
                if (content is IGreedyContent greedyContent) greedyContents.Add(greedyContent);
            }

            for (var i = greedyContents.Count - 1; i >= 0; i--) greedyContents[i].AbsorbContent(_contents);
        }

        internal virtual List<IPathContent> PathList
        {
            get
            {
                if (_pathContents == null)
                {
                    _pathContents = new List<IPathContent>();
                    for (var i = 0; i < _contents.Count; i++)
                        if (_contents[i] is IPathContent content)
                            _pathContents.Add(content);
                }

                return _pathContents;
            }
        }

        internal virtual Matrix3X3 TransformationMatrix
        {
            get
            {
                if (_transformAnimation != null) return _transformAnimation.Matrix;
                _matrix.Reset();
                return _matrix;
            }
        }

        public virtual string Name { get; }

        public virtual void SetContents(List<IContent> contentsBefore, List<IContent> contentsAfter)
        {
            // Do nothing with contents after.
            var myContentsBefore = new List<IContent>(contentsBefore.Count + _contents.Count);
            myContentsBefore.AddRange(contentsBefore);

            for (var i = _contents.Count - 1; i >= 0; i--)
            {
                var content = _contents[i];
                content.SetContents(myContentsBefore, _contents.Take(i + 1).ToList());
                myContentsBefore.Add(content);
            }
        }

        public virtual void Draw(BitmapCanvas canvas, Matrix3X3 parentMatrix, byte parentAlpha)
        {
            _matrix.Set(parentMatrix);
            byte alpha;
            if (_transformAnimation != null)
            {
                _matrix = MatrixExt.PreConcat(_matrix, _transformAnimation.Matrix);
                alpha = (byte) (_transformAnimation.Opacity.Value / 100f * parentAlpha / 255f * 255);
            }
            else
            {
                alpha = parentAlpha;
            }

            for (var i = _contents.Count - 1; i >= 0; i--)
            {
                var drawingContent = _contents[i] as IDrawingContent;
                drawingContent?.Draw(canvas, _matrix, alpha);
            }
        }

        public virtual void GetBounds(ref Rect outBounds, Matrix3X3 parentMatrix)
        {
            _matrix.Set(parentMatrix);
            if (_transformAnimation != null) _matrix = MatrixExt.PreConcat(_matrix, _transformAnimation.Matrix);
            RectExt.Set(ref _rect, 0, 0, 0, 0);
            for (var i = _contents.Count - 1; i >= 0; i--)
                if (_contents[i] is IDrawingContent drawingContent)
                {
                    drawingContent.GetBounds(ref _rect, _matrix);
                    if (outBounds.IsEmpty)
                        RectExt.Set(ref outBounds, _rect);
                    else
                        RectExt.Set(ref outBounds,
                            (float) Math.Min(outBounds.Left, _rect.Left),
                            (float) Math.Min(outBounds.Top, _rect.Top),
                            (float) Math.Max(outBounds.Right, _rect.Right),
                            (float) Math.Max(outBounds.Bottom, _rect.Bottom));
                }
        }

        public void ResolveKeyPath(KeyPath keyPath, int depth, List<KeyPath> accumulator, KeyPath currentPartialKeyPath)
        {
            if (!keyPath.Matches(Name, depth)) return;

            if (!"__container".Equals(Name))
            {
                currentPartialKeyPath = currentPartialKeyPath.AddKey(Name);

                if (keyPath.FullyResolvesTo(Name, depth)) accumulator.Add(currentPartialKeyPath.Resolve(this));
            }

            if (keyPath.PropagateToChildren(Name, depth))
            {
                var newDepth = depth + keyPath.IncrementDepthBy(Name, depth);
                for (var i = 0; i < _contents.Count; i++)
                {
                    var content = _contents[i];
                    if (content is IKeyPathElement element)
                        element.ResolveKeyPath(keyPath, newDepth, accumulator, currentPartialKeyPath);
                }
            }
        }

        public void AddValueCallback<T>(LottieProperty property, ILottieValueCallback<T> callback)
        {
            _transformAnimation?.ApplyValueCallback(property, callback);
        }

        public Path Path
        {
            get
            {
                // TODO: cache this somehow.
                _matrix.Reset();
                if (_transformAnimation != null) _matrix.Set(_transformAnimation.Matrix);
                _path.Reset();
                for (var i = _contents.Count - 1; i >= 0; i--)
                    if (_contents[i] is IPathContent pathContent)
                        _path.AddPath(pathContent.Path, _matrix);
                return _path;
            }
        }

        private static List<IContent> ContentsFromModels(Lottie drawable, BaseLayer layer,
            List<IContentModel> contentModels)
        {
            var contents = new List<IContent>(contentModels.Count);
            for (var i = 0; i < contentModels.Count; i++)
            {
                var content = contentModels[i].ToContent(drawable, layer);
                if (content != null) contents.Add(content);
            }

            return contents;
        }

        internal static AnimatableTransform FindTransform(List<IContentModel> contentModels)
        {
            for (var i = 0; i < contentModels.Count; i++)
            {
                var contentModel = contentModels[i];
                if (contentModel is AnimatableTransform animatableTransform) return animatableTransform;
            }

            return null;
        }
    }
}