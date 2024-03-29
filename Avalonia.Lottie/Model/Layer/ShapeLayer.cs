﻿using System.Collections.Generic;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Model.Content;

namespace Avalonia.Lottie.Model.Layer
{
    internal class ShapeLayer : BaseLayer
    {
        private readonly ContentGroup _contentGroup;

        internal ShapeLayer(Lottie lottie, Layer layerModel) : base(lottie, layerModel)
        {
            // Naming this __container allows it to be ignored in KeyPath matching. 
            var shapeGroup = new ShapeGroup("__container", layerModel.Shapes);
            _contentGroup = new ContentGroup(lottie, this, shapeGroup);
            _contentGroup.SetContents(new List<IContent>(), new List<IContent>());
        }

        public override void DrawLayer(LottieCanvas canvas, Matrix parentMatrix, byte parentAlpha)
        {
            _contentGroup.Draw(canvas, parentMatrix, parentAlpha);
        }

        public override void GetBounds(ref Rect outBounds, Matrix parentMatrix)
        {
            base.GetBounds(ref outBounds, parentMatrix);
            _contentGroup.GetBounds(ref outBounds, BoundsMatrix);
        }

        internal override void ResolveChildKeyPath(KeyPath keyPath, int depth, List<KeyPath> accumulator,
            KeyPath currentPartialKeyPath)
        {
            _contentGroup.ResolveKeyPath(keyPath, depth, accumulator, currentPartialKeyPath);
        }
    }
}