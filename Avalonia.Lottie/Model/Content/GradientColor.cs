﻿using System;
using Avalonia.Lottie.Utils;
using Avalonia.Media;

namespace Avalonia.Lottie.Model.Content
{
    public class GradientColor
    {
        private readonly Color[] _colors;
        private readonly double [] _positions;

        internal GradientColor(double[] positions, Color[] colors)
        {
            _positions = positions;
            _colors = colors;
        }

        internal virtual double [] Positions => _positions;

        internal virtual Color[] Colors => _colors;

        internal virtual int Size => _colors.Length;

        internal virtual void Lerp(GradientColor gc1, GradientColor gc2, double  progress)
        {
            if (gc1._colors.Length != gc2._colors.Length)
                throw new ArgumentException("Cannot interpolate between gradients. Lengths vary (" +
                                            gc1._colors.Length + " vs " + gc2._colors.Length + ")");

            for (var i = 0; i < gc1._colors.Length; i++)
            {
                _positions[i] = MiscUtils.Lerp(gc1._positions[i], gc2._positions[i], progress);

                var gamma = GammaEvaluator.Evaluate(progress, gc1._colors[i], gc2._colors[i]);

                _colors[i] = gamma;
            }
        }
    }
}