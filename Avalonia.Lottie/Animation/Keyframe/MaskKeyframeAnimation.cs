﻿using System.Collections.Generic;
using Avalonia.Lottie.Model.Content;

namespace Avalonia.Lottie.Animation.Keyframe
{
    internal class MaskKeyframeAnimation
    {
        private readonly List<IBaseKeyframeAnimation<ShapeData, Path>> _maskAnimations;
        private readonly List<IBaseKeyframeAnimation<int?, int?>> _opacityAnimations;

        internal MaskKeyframeAnimation(List<Mask> masks)
        {
            Masks = masks;
            _maskAnimations = new List<IBaseKeyframeAnimation<ShapeData, Path>>(masks.Count);
            _opacityAnimations = new List<IBaseKeyframeAnimation<int?, int?>>(masks.Count);
            for (var i = 0; i < masks.Count; i++)
            {
                _maskAnimations.Add(masks[i].MaskPath.CreateAnimation());
                var opacity = masks[i].Opacity;
                _opacityAnimations.Add(opacity.CreateAnimation());
            }
        }

        internal virtual List<Mask> Masks { get; }

        internal virtual List<IBaseKeyframeAnimation<ShapeData, Path>> MaskAnimations => _maskAnimations;

        internal virtual List<IBaseKeyframeAnimation<int?, int?>> OpacityAnimations => _opacityAnimations;
    }
}