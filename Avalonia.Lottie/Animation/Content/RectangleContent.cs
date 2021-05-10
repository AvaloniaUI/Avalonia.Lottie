/* Unmerged change from project 'Avalonia.Lottie (netcoreapp3.0)'
Before:
using Avalonia.Lottie.Value;
After:
using Avalonia.Lottie.Value;


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
    internal class RectangleContent : IPathContent, IKeyPathElementContent
    {
        private readonly IBaseKeyframeAnimation<float?, float?> _cornerRadiusAnimation;

        private readonly Lottie _lottie;
        private readonly Path _path = new();
        private readonly IBaseKeyframeAnimation<Vector2?, Vector2?> _positionAnimation;
        private readonly IBaseKeyframeAnimation<Vector2?, Vector2?> _sizeAnimation;
        private bool _isPathValid;
        private Rect _rect;

        private TrimPathContent _trimPath;

        internal RectangleContent(Lottie lottie, BaseLayer layer, RectangleShape rectShape)
        {
            Name = rectShape.Name;
            _lottie = lottie;
            _positionAnimation = rectShape.Position.CreateAnimation();
            _sizeAnimation = rectShape.Size.CreateAnimation();
            _cornerRadiusAnimation = rectShape.CornerRadius.CreateAnimation();

            layer.AddAnimation(_positionAnimation);
            layer.AddAnimation(_sizeAnimation);
            layer.AddAnimation(_cornerRadiusAnimation);

            _positionAnimation.ValueChanged += OnValueChanged;
            _sizeAnimation.ValueChanged += OnValueChanged;
            _cornerRadiusAnimation.ValueChanged += OnValueChanged;
        }

        public void ResolveKeyPath(KeyPath keyPath, int depth, List<KeyPath> accumulator, KeyPath currentPartialKeyPath)
        {
            MiscUtils.ResolveKeyPath(keyPath, depth, accumulator, currentPartialKeyPath, this);
        }

        public void AddValueCallback<T>(LottieProperty property, ILottieValueCallback<T> callback)
        {
        }

        public string Name { get; }

        public void SetContents(List<IContent> contentsBefore, List<IContent> contentsAfter)
        {
            for (var i = 0; i < contentsBefore.Count; i++)
                if (contentsBefore[i] is TrimPathContent trimPathContent &&
                    trimPathContent.Type == ShapeTrimPath.Type.Simultaneously)
                {
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

                var size = _sizeAnimation.Value;
                var halfWidth = size.Value.X / 2f;
                var halfHeight = size.Value.Y / 2f;
                var radius = _cornerRadiusAnimation?.Value ?? 0f;
                var maxRadius = Math.Min(halfWidth, halfHeight);
                if (radius > maxRadius) radius = maxRadius;

                // Draw the rectangle top right to bottom left.
                var position = _positionAnimation.Value.Value;

                _path.MoveTo(position.X + halfWidth, position.Y - halfHeight + radius);

                _path.LineTo(position.X + halfWidth, position.Y + halfHeight - radius);

                if (radius > 0)
                {
                    RectExt.Set(ref _rect, position.X + halfWidth - 2 * radius, position.Y + halfHeight - 2 * radius,
                        position.X + halfWidth, position.Y + halfHeight);
                    _path.ArcTo(position.X + halfWidth, position.Y + halfHeight - radius, _rect, 0, 90);
                }


                /* Unmerged change from project 'Avalonia.Lottie (netcoreapp3.0)'
                Before:
                                _path.LineTo(position.X - halfWidth + radius, position.Y + halfHeight);

                                if (radius > 0)
                After:
                                _path.LineTo(position.X - halfWidth + radius, position.Y + halfHeight);

                                if (radius > 0)
                */
                _path.LineTo(position.X - halfWidth + radius, position.Y + halfHeight);

                if (radius > 0)
                {
                    RectExt.Set(ref _rect, position.X - halfWidth, position.Y + halfHeight - 2 * radius,
                        position.X - halfWidth + 2 * radius, position.Y + halfHeight);
                    _path.ArcTo(position.X - halfWidth + radius, position.Y + halfHeight, _rect, 90, 90);
                }

                _path.LineTo(position.X - halfWidth, position.Y - halfHeight + radius);

                if (radius > 0)
                {
                    RectExt.Set(ref _rect, position.X - halfWidth, position.Y - halfHeight,
                        position.X - halfWidth + 2 * radius, position.Y - halfHeight + 2 * radius);
                    _path.ArcTo(position.X - halfWidth, position.Y - halfHeight + radius, _rect, 180, 90);

                    /* Unmerged change from project 'Avalonia.Lottie (netcoreapp3.0)'
                    Before:
                                    }

                                    _path.LineTo(position.X + halfWidth - radius, position.Y - halfHeight);
                    After:
                                    }

                                    _path.LineTo(position.X + halfWidth - radius, position.Y - halfHeight);
                    */
                }

                _path.LineTo(position.X + halfWidth - radius, position.Y - halfHeight);

                if (radius > 0)
                {
                    RectExt.Set(ref _rect, position.X + halfWidth - 2 * radius, position.Y - halfHeight,
                        position.X + halfWidth, position.Y - halfHeight + 2 * radius);
                    _path.ArcTo(position.X + halfWidth - radius, position.Y - halfHeight, _rect, 270, 90);
                }

                _path.Close();

                Utils.Utils.ApplyTrimPathIfNeeded(_path, _trimPath);

                _isPathValid = true;
                return _path;
            }
        }

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