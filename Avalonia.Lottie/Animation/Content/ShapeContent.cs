﻿using System;
using System.Collections.Generic;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Model.Content;
using Avalonia.Lottie.Model.Layer;

namespace Avalonia.Lottie.Animation.Content
{
    internal class ShapeContent : IPathContent
    {
        private readonly Lottie _lottie;
        private readonly Path _path = new();
        private readonly IBaseKeyframeAnimation<ShapeData, Path> _shapeAnimation;

        private bool _isPathValid;
        private TrimPathContent _trimPath;

        internal ShapeContent(Lottie lottie, BaseLayer layer, ShapePath shape)
        {
            Name = shape.Name;
            _lottie = lottie;
            _shapeAnimation = shape.GetShapePath().CreateAnimation();
            layer.AddAnimation(_shapeAnimation);
            _shapeAnimation.ValueChanged += OnValueChanged;
        }

        public void SetContents(List<IContent> contentsBefore, List<IContent> contentsAfter)
        {
            for (var i = 0; i < contentsBefore.Count; i++)
                if (contentsBefore[i] is TrimPathContent trimPathContent &&
                    trimPathContent.Type == ShapeTrimPath.Type.Simultaneously)
                {
                    // Trim path individually will be handled by the stroke where paths are combined.
                    _trimPath = trimPathContent;
                    _trimPath.ValueChanged += OnValueChanged;
                }
        }

        public Path Path
        {
            get
            {
                if (_isPathValid) return _path;

                _path.Reset();

                _path.Set(_shapeAnimation.Value);
                _path.FillType = PathFillType.EvenOdd;

                Utils.Utils.ApplyTrimPathIfNeeded(_path, _trimPath);

                _isPathValid = true;
                return _path;
            }
        }

        public string Name { get; }

        private void OnValueChanged(object sender, EventArgs eventArgs)
        {
            Invalidate();
        }

        private void Invalidate()
        {
            _isPathValid = false;
            _lottie.InvalidateSelf();
        }
    }
}