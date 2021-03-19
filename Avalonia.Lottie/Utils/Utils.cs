﻿using System;
using System.Numerics;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Media;

//TODO: OID: Totally replaced with new file

namespace Avalonia.Lottie.Utils
{
    public static class Utils
    {
        public const int SecondInNanos = 1000000000;
        private static  Path _tempPath = new();
        private static  Path _tempPath2 = new();
        private static Vector2[] _points = new Vector2[2];
        private static readonly float Sqrt2 = (float) Math.Sqrt(2);
        private static float _dpScale = -1;
        private static float _dpi = -1;

        static Utils()
        {
            DpScale();
            Dpi();
        }

        internal static Path CreatePath(Vector2 startPoint, Vector2 endPoint, Vector2? cp1, Vector2? cp2)
        {
            var path = new Path();
            path.MoveTo(startPoint.X, startPoint.Y);

            if (cp1.HasValue && cp2.HasValue && (cp1.Value.LengthSquared() != 0 || cp2.Value.LengthSquared() != 0))
                path.CubicTo(startPoint.X + cp1.Value.X, startPoint.Y + cp1.Value.Y, endPoint.X + cp2.Value.X,
                    endPoint.Y + cp2.Value.Y, endPoint.X, endPoint.Y);
            else
                path.LineTo(endPoint.X, endPoint.Y);
            return path;
        }

        public static void CloseQuietly(this IDisposable closeable)
        {
            if (closeable != null)
                try
                {
                    closeable.Dispose();
                }
                //catch (RuntimeException rethrown)
                //{
                //    throw rethrown;
                //}
                catch (Exception)
                {
                    // Really quietly
                }
        }

        internal static float GetScale(Matrix3X3 matrix)
        {
            _points[0].X = 0;
            _points[0].Y = 0;
            _points[1].X = Sqrt2;
            _points[1].Y = Sqrt2;
            // Use sqrt(2) so that the hypotenuse is of length 1.
            matrix.MapPoints(ref _points);
            var dx = _points[1].X - _points[0].X;
            var dy = _points[1].Y - _points[0].Y;

            // TODO: figure out why the result needs to be divided by 2.
            return (float) MathExt.Hypot(dx, dy) / 2f;
        }

        internal static void ApplyTrimPathIfNeeded(Path path, TrimPathContent trimPath)
        {
            if (trimPath == null) return;
            ApplyTrimPathIfNeeded(path, trimPath.Start.Value.Value / 100f, trimPath.End.Value.Value / 100f,
                trimPath.Offset.Value.Value / 360f);
        }

        internal static void ApplyTrimPathIfNeeded(Path path, float startValue, float endValue, float offsetValue)
        {
            LottieLog.BeginSection("applyTrimPathIfNeeded");
            using (var pathMeasure = new PathMeasure(path))
            {
                var length = pathMeasure.Length;
                if (length > 0)
                {
                    
                }
                
                if (startValue == 1f && endValue == 0f)
                {
                    LottieLog.EndSection("applyTrimPathIfNeeded");
                    return;
                }

                if (length < 1f || Math.Abs(endValue - startValue - 1) < .01)
                {
                    LottieLog.EndSection("applyTrimPathIfNeeded");
                    return;
                }

                var start = length * startValue;
                var end = length * endValue;
                var newStart = Math.Min(start, end);
                var newEnd = Math.Max(start, end);

                var offset = offsetValue * length;
                newStart += offset;
                newEnd += offset;

                // If the trim path has rotated around the path, we need to shift it back.
                if (newStart >= length && newEnd >= length)
                {
                    newStart = MiscUtils.FloorMod(newStart, length);
                    newEnd = MiscUtils.FloorMod(newEnd, length);
                }

                if (newStart < 0) newStart = MiscUtils.FloorMod(newStart, length);
                if (newEnd < 0) newEnd = MiscUtils.FloorMod(newEnd, length);

                // If the start and end are equals, return an empty path.
                if (newStart == newEnd)
                {
                    path.Reset();
                    LottieLog.EndSection("applyTrimPathIfNeeded");
                    return;
                }

                if (newStart >= newEnd) newStart -= length;

                _tempPath.Reset();
                pathMeasure.GetSegment(newStart, newEnd,ref _tempPath, true);

                if (newEnd > length)
                {
                    _tempPath2.Reset();
                    pathMeasure.GetSegment(0, newEnd % length, ref _tempPath2, true);
                    _tempPath.AddPath(_tempPath2);
                }
                else if (newStart < 0)
                {
                    _tempPath2.Reset();
                    pathMeasure.GetSegment(length + newStart, length, ref _tempPath2, true);
                    _tempPath.AddPath(_tempPath2);
                }
            }

            path.Set(_tempPath);
            LottieLog.EndSection("applyTrimPathIfNeeded");
        }

        public static Color GetSolidColorBrush(string hex)
        {
            var index = 1; // Skip '#'
            // '#AARRGGBB'
            byte a = 255;
            if (hex.Length == 9)
            {
                a = (byte) Convert.ToUInt32(hex.Substring(index, 2), 16);
                index += 2;
            }

            var r = (byte) Convert.ToUInt32(hex.Substring(index, 2), 16);
            index += 2;
            var g = (byte) Convert.ToUInt32(hex.Substring(index, 2), 16);
            index += 2;
            var b = (byte) Convert.ToUInt32(hex.Substring(index, 2), 16);
            return new Color(a, r, g, b);
        }

        public static bool IsAtLeastVersion(int major, int minor, int patch, int minMajor, int minMinor, int minPatch)
        {
            if (major < minMajor) return false;
            if (major > minMajor) return true;

            if (minor < minMinor) return false;
            if (minor > minMinor) return true;

            return patch >= minPatch;
        }

        internal static int HashFor(float a, float b, float c, float d)
        {
            var result = 17;
            if (a != 0) result = (int) (31 * result * a);
            if (b != 0) result = (int) (31 * result * b);
            if (c != 0) result = (int) (31 * result * c);
            if (d != 0) result = (int) (31 * result * d);
            return result;
        }

        public static float DpScale()
        {
            if (_dpScale == -1) _dpScale = (int) DisplayInformation.GetForCurrentView().ResolutionScale / 100f;
            return _dpScale;
        }

        public static float Dpi()
        {
            if (_dpi == -1) _dpi = (int) DisplayInformation.GetForCurrentView().LogicalDpi;
            return _dpi;
        }
    }

    public class DisplayInformation
    {
        public ResolutionScale ResolutionScale { get; internal set; }
        public double ScreenWidthInRawPixels { get; set; }

        public double ScreenHeightInRawPixels { get; set; }

        //TODO Return correct dpi
        public float LogicalDpi { get; set; }

        internal static DisplayInformation GetForCurrentView()
        {
            return new()
            {
                ResolutionScale = ResolutionScale.Scale100Percent, ScreenHeightInRawPixels = 500,
                ScreenWidthInRawPixels = 2000, LogicalDpi = 1f
            };
        }
    }

    //
    // Zusammenfassung:
    //     Beschreibt den Skalierungsfaktor der immersiven Umgebung ab. Der Skalierungsfaktor
    //     wird vom Betriebssystem als Reaktion auf Bildschirme mit hoher Pixeldichte bestimmt.
    public enum ResolutionScale
    {
        //
        // Zusammenfassung:
        //     Gibt an, dass die Skalierung einer Anzeige ungültig ist.
        Invalid = 0,

        //
        // Zusammenfassung:
        //     Gibt für die Skalierung einer Anzeige den Wert 100 Prozent an. Dieser Prozentsatz
        //     gibt an, dass eine minimale Bildschirmauflösung für Windows insgesamt 1024x768
        //     beträgt.
        Scale100Percent = 100,

        //
        // Zusammenfassung:
        //     Dieser Wert wird nicht verwendet.
        Scale120Percent = 120,

        //
        // Zusammenfassung:
        //     Gibt für die Skalierung einer Anzeige den Wert 125 Prozent an.
        Scale125Percent = 125,

        //
        // Zusammenfassung:
        //     Gibt für die Skalierung einer Anzeige den Wert 140 Prozent an. Dieser Prozentsatz
        //     gibt an, dass eine minimale Bildschirmauflösung, um auf 140 zu skalieren, 1440x1080
        //     beträgt.
        Scale140Percent = 140,

        //
        // Zusammenfassung:
        //     Gilt für Windows Phone 8.
        Scale150Percent = 150,

        //
        // Zusammenfassung:
        //     Gilt für Windows Phone 8.
        Scale160Percent = 160,

        //
        // Zusammenfassung:
        //     Gibt für die Skalierung einer Anzeige den Wert 175 Prozent an.
        Scale175Percent = 175,

        //
        // Zusammenfassung:
        //     Gibt für die Skalierung einer Anzeige den Wert 180 Prozent an. Dieser Prozentsatz
        //     gibt an, dass eine minimale Bildschirmauflösung, um auf 180 zu skalieren, 1920x1440
        //     beträgt.
        Scale180Percent = 180,

        //
        // Zusammenfassung:
        //     Gibt für die Skalierung einer Anzeige den Wert 200 Prozent an.
        Scale200Percent = 200,

        //
        // Zusammenfassung:
        //     Dieser Wert wird nicht verwendet.
        Scale225Percent = 225,

        //
        // Zusammenfassung:
        //     Gibt für die Skalierung einer Anzeige den Wert 250 Prozent an.
        Scale250Percent = 250,

        //
        // Zusammenfassung:
        //     Gibt für die Skalierung einer Anzeige den Wert 300 Prozent an.
        Scale300Percent = 300,

        //
        // Zusammenfassung:
        //     Gibt für die Skalierung einer Anzeige den Wert 350 Prozent an.
        Scale350Percent = 350,

        //
        // Zusammenfassung:
        //     Gibt für die Skalierung einer Anzeige den Wert 400 Prozent an.
        Scale400Percent = 400,

        //
        // Zusammenfassung:
        //     Gibt für die Skalierung einer Anzeige den Wert 450 Prozent an.
        Scale450Percent = 450,

        //
        // Zusammenfassung:
        //     Gibt für die Skalierung einer Anzeige den Wert 500 Prozent an.
        Scale500Percent = 500
    }
}