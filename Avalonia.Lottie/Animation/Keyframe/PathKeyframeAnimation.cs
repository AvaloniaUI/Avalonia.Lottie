﻿using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Animation.Keyframe
{
    internal class PathKeyframeAnimation : KeyframeAnimation<Vector?>, IDisposable
    {
        private PathMeasure _pathMeasure;
        private PathKeyframe _pathMeasureKeyframe;

        internal PathKeyframeAnimation(List<Keyframe<Vector?>> keyframes)
            : base(keyframes)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override Vector? GetValue(Keyframe<Vector?> keyframe, double  keyframeProgress)
        {
            var pathKeyframe = (PathKeyframe) keyframe;
            var path = pathKeyframe.Path;
            if (path == null || path.Contours.Count == 0) return keyframe.StartValue;

            if (ValueCallback != null)
                return ValueCallback.GetValueInternal(pathKeyframe.StartFrame.Value, pathKeyframe.EndFrame.Value,
                    pathKeyframe.StartValue, pathKeyframe.EndValue, LinearCurrentKeyframeProgress,
                    keyframeProgress, Progress);

            if (_pathMeasureKeyframe != pathKeyframe)
            {
                _pathMeasure?.Dispose();
                _pathMeasure = new PathMeasure(path);
                _pathMeasureKeyframe = pathKeyframe;
            }

            return _pathMeasure.GetPosTan(keyframeProgress * _pathMeasure.Length);
        }

        private void Dispose(bool disposing)
        {
            if (_pathMeasure != null)
            {
                _pathMeasure.Dispose(disposing);
                _pathMeasure = null;
            }
        }

        ~PathKeyframeAnimation()
        {
            Dispose(false);
        }
    }
}