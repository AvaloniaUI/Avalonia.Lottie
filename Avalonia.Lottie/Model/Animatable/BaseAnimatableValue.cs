﻿using System.Collections.Generic;
using System.Text;
using Avalonia.Lottie.Animation.Keyframe;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Model.Animatable
{
    public abstract class BaseAnimatableValue<TV, TO> : IAnimatableValue<TV, TO>
    {
        internal readonly List<Keyframe<TV>> Keyframes;

        /// <summary>
        ///     Create a default static animatable path.
        /// </summary>
        internal BaseAnimatableValue(TV value) : this(new List<Keyframe<TV>> {new(value)})
        {
        }

        internal BaseAnimatableValue(List<Keyframe<TV>> keyframes)
        {
            Keyframes = keyframes;
        }

        public abstract IBaseKeyframeAnimation<TV, TO> CreateAnimation();

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Keyframes.Count > 0) sb.Append("values=").Append("[" + string.Join(",", Keyframes) + "]");
            return sb.ToString();
        }
    }
}