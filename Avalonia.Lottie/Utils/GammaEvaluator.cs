﻿using System;
using Avalonia.Media;

namespace Avalonia.Lottie.Utils
{
    /// <summary>
    ///     Use this instead of ArgbEvaluator because it interpolates through the gamma color
    ///     space which looks better to us humans.
    ///     <para>
    ///         Writted by Romain Guy and Francois Blavoet.
    ///         https://androidstudygroup.slack.com/archives/animation/p1476461064000335
    ///     </para>
    /// </summary>
    internal static class GammaEvaluator
    {
        // Opto-electronic conversion function for the sRGB color space
        // Takes a gamma-encoded sRGB value and converts it to a linear sRGB value
        private static double  OECF_sRGB(double linear)
        {
            // IEC 61966-2-1:1999
            return linear <= 0.0031308f ? linear * 12.92f :  (Math.Pow(linear, 1.0f / 2.4f) * 1.055f - 0.055f);
        }

        // Electro-optical conversion function for the sRGB color space
        // Takes a linear sRGB value and converts it to a gamma-encoded sRGB value
        private static double  EOCF_sRGB(double srgb)
        {
            // IEC 61966-2-1:1999
            return srgb <= 0.04045f ? srgb / 12.92f :  Math.Pow((srgb + 0.055f) / 1.055f, 2.4f);
        }

        internal static Color Evaluate(double fraction, Color startColor, Color endColor)
        {
            return Evaluate(fraction,
                startColor.A / 255.0f, startColor.R / 255.0f, startColor.G / 255.0f, startColor.B / 255.0f,
                endColor.A / 255.0f, endColor.R / 255.0f, endColor.G / 255.0f, endColor.B / 255.0f);
        }

        //internal static int evaluate(double fraction, int startInt, int endInt)
        //{
        //    double  startA = ((startInt >> 24) & 0xff) / 255.0f;
        //    double  startR = ((startInt >> 16) & 0xff) / 255.0f;
        //    double  startG = ((startInt >> 8) & 0xff) / 255.0f;
        //    double  startB = (startInt & 0xff) / 255.0f;

        //    double  endA = ((endInt >> 24) & 0xff) / 255.0f;
        //    double  endR = ((endInt >> 16) & 0xff) / 255.0f;
        //    double  endG = ((endInt >> 8) & 0xff) / 255.0f;
        //    double  endB = (endInt & 0xff) / 255.0f;

        //    return evaluate(fraction, startA, startR, startG, startB, endA, endR, endG, endB);
        //}

        private static Color Evaluate(double fraction,
            double  startA, double  startR, double  startG, double  startB,
            double  endA, double  endR, double  endG, double  endB)
        {
            // convert from sRGB to linear
            startR = EOCF_sRGB(startR);
            startG = EOCF_sRGB(startG);
            startB = EOCF_sRGB(startB);

            endR = EOCF_sRGB(endR);
            endG = EOCF_sRGB(endG);
            endB = EOCF_sRGB(endB);

            // compute the interpolated color in linear space
            var a = startA + fraction * (endA - startA);
            var r = startR + fraction * (endR - startR);
            var g = startG + fraction * (endG - startG);
            var b = startB + fraction * (endB - startB);

            // convert back to sRGB in the [0..255] range
            a = a * 255.0f;
            r = OECF_sRGB(r) * 255.0f;
            g = OECF_sRGB(g) * 255.0f;
            b = OECF_sRGB(b) * 255.0f;

            return new Color((byte) Math.Round(a), (byte) Math.Round(r), (byte) Math.Round(g), (byte) Math.Round(b));
        }
    }
}