﻿using System.Numerics;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Model.Animatable;
using Avalonia.Lottie.Model.Layer;

namespace Avalonia.Lottie.Model.Content
{
    public class CircleShape : IContentModel
    {
        public CircleShape(string name, IAnimatableValue<Vector?, Vector?> position, AnimatablePointValue size,
            bool isReversed)
        {
            Name = name;
            Position = position;
            Size = size;
            IsReversed = isReversed;
        }

        internal string Name { get; }

        public IAnimatableValue<Vector?, Vector?> Position { get; }

        public AnimatablePointValue Size { get; }

        public bool IsReversed { get; }

        public IContent ToContent(Lottie drawable, BaseLayer layer)
        {
            return new EllipseContent(drawable, layer, this);
        }
    }
}