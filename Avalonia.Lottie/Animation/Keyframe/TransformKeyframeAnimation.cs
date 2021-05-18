using System;
using System.Numerics;
using Avalonia.Lottie.Model;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Layer;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Animation.Keyframe
{
    public class TransformKeyframeAnimation
    {
        private readonly IBaseKeyframeAnimation<Vector?, Vector?> _anchorPoint;
        private readonly IBaseKeyframeAnimation<float?, float?> _endOpacity;
        private readonly IBaseKeyframeAnimation<int?, int?> _opacity;
        private readonly IBaseKeyframeAnimation<Vector?, Vector?> _position;
        private readonly IBaseKeyframeAnimation<float?, float?> _rotation;
        private readonly IBaseKeyframeAnimation<ScaleXy, ScaleXy> _scale;

        // Used for repeaters 
        private readonly IBaseKeyframeAnimation<float?, float?> _startOpacity;
        private Matrix3X3 _matrix = Matrix3X3.CreateIdentity();

        internal TransformKeyframeAnimation(AnimatableTransform animatableTransform)
        {
            _anchorPoint = animatableTransform.AnchorPoint.CreateAnimation();
            _position = animatableTransform.Position.CreateAnimation();
            _scale = animatableTransform.Scale.CreateAnimation();
            _rotation = animatableTransform.Rotation.CreateAnimation();
            _opacity = animatableTransform.Opacity.CreateAnimation();
            _startOpacity = animatableTransform.StartOpacity?.CreateAnimation();
            _endOpacity = animatableTransform.EndOpacity?.CreateAnimation();
        }

        public double  Progress
        {
            set
            {
                _anchorPoint.Progress = value;
                _position.Progress = value;
                _scale.Progress = value;
                _rotation.Progress = value;
                _opacity.Progress = value;
                if (_startOpacity != null) _startOpacity.Progress = value;
                if (_endOpacity != null) _endOpacity.Progress = value;
            }
        }

        internal virtual IBaseKeyframeAnimation<int?, int?> Opacity => _opacity;

        internal virtual IBaseKeyframeAnimation<float?, float?> StartOpacity => _startOpacity;

        internal virtual IBaseKeyframeAnimation<float?, float?> EndOpacity => _endOpacity;

        internal virtual Matrix3X3 Matrix
        {
            get
            {
                _matrix.Reset();
                var position = _position.Value;
                if (position != null && (position.Value.X != 0 || position.Value.Y != 0))
                    _matrix = MatrixExt.PreTranslate(_matrix, position.Value.X, position.Value.Y);

                if (_rotation.Value.HasValue && _rotation.Value.Value != 0f)
                    _matrix = MatrixExt.PreRotate(_matrix, _rotation.Value.Value);

                var scaleTransform = _scale.Value;
                if (scaleTransform != null && (scaleTransform.ScaleX != 1f || scaleTransform.ScaleY != 1f))
                    _matrix = MatrixExt.PreScale(_matrix, scaleTransform.ScaleX, scaleTransform.ScaleY);

                var anchorPoint = _anchorPoint.Value;
                if (anchorPoint != null && (anchorPoint.Value.X != 0 || anchorPoint.Value.Y != 0))
                    _matrix = MatrixExt.PreTranslate(_matrix, -anchorPoint.Value.X, -anchorPoint.Value.Y);
                return _matrix;
            }
        }

        internal virtual void AddAnimationsToLayer(BaseLayer layer)
        {
            layer.AddAnimation(_anchorPoint);
            layer.AddAnimation(_position);
            layer.AddAnimation(_scale);
            layer.AddAnimation(_rotation);
            layer.AddAnimation(_opacity);
            if (_startOpacity != null) layer.AddAnimation(_startOpacity);
            if (_endOpacity != null) layer.AddAnimation(_endOpacity);
        }

        internal event EventHandler ValueChanged
        {
            add
            {
                _anchorPoint.ValueChanged += value;
                _position.ValueChanged += value;
                _scale.ValueChanged += value;
                _rotation.ValueChanged += value;
                _opacity.ValueChanged += value;
                if (_startOpacity != null) _startOpacity.ValueChanged += value;
                if (_endOpacity != null) _endOpacity.ValueChanged += value;
            }
            remove
            {
                _anchorPoint.ValueChanged -= value;
                _position.ValueChanged -= value;
                _scale.ValueChanged -= value;
                _rotation.ValueChanged -= value;
                _opacity.ValueChanged -= value;
                if (_startOpacity != null) _startOpacity.ValueChanged -= value;
                if (_endOpacity != null) _endOpacity.ValueChanged -= value;
            }
        }

        /**
         * TODO: see if we can use this for the main get_Matrix method.
         */
        internal Matrix3X3 GetMatrixForRepeater(double amount)
        {
            var position = _position.Value;
            var anchorPoint = _anchorPoint.Value;
            var scale = _scale.Value;
            var rotation = _rotation.Value.Value;

            _matrix.Reset();
            _matrix = MatrixExt.PreTranslate(_matrix, position.Value.X * amount, position.Value.Y * amount);
            _matrix = MatrixExt.PreScale(_matrix,
                 Math.Pow(scale.ScaleX, amount),
                 Math.Pow(scale.ScaleY, amount));
            _matrix = MatrixExt.PreRotate(_matrix, rotation * amount, anchorPoint.Value.X, anchorPoint.Value.Y);

            return _matrix;
        }

        /// <summary>
        ///     Returns whether the callback was applied.
        /// </summary>
        public bool ApplyValueCallback<T>(LottieProperty property, ILottieValueCallback<T> callback)
        {
            if (property == LottieProperty.TransformAnchorPoint)
                _anchorPoint.SetValueCallback((ILottieValueCallback<Vector?>) callback);
            else if (property == LottieProperty.TransformPosition)
                _position.SetValueCallback((ILottieValueCallback<Vector?>) callback);
            else if (property == LottieProperty.TransformScale)
                _scale.SetValueCallback((ILottieValueCallback<ScaleXy>) callback);
            else if (property == LottieProperty.TransformRotation)
                _rotation.SetValueCallback((ILottieValueCallback<float?>) callback);
            else if (property == LottieProperty.TransformOpacity)
                _opacity.SetValueCallback((ILottieValueCallback<int?>) callback);
            else if (property == LottieProperty.TransformStartOpacity && _startOpacity != null)
                _startOpacity.SetValueCallback((ILottieValueCallback<float?>) callback);
            else if (property == LottieProperty.TransformEndOpacity && _endOpacity != null)
                _endOpacity.SetValueCallback((ILottieValueCallback<float?>) callback);
            else
                return false;
            return true;
        }
    }
}