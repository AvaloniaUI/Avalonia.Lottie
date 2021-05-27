using System;
using Avalonia.Media;

namespace Avalonia.Lottie.Animation.Content
{
    public class Paint : IDisposable
    {
        public enum PaintStyle
        {
            Fill,
            Stroke
        }

        public static int AntiAliasFlag = 0b01;
        public static int FilterBitmapFlag = 0b10;
        private bool disposedValue;

        public Paint(int flags)
        {
            Flags = flags;
        }

        public Paint()
            : this(0)
        {
        }

        public int Flags { get; }

        public byte Alpha
        {
            get => Color.A;
            set
            {
                var color = Color;
                Color = new Color(value, Color.R, Color.G, Color.B);
            }
        }

        public Color Color { get; set; } = Colors.Transparent;
        public PaintStyle Style { get; set; }
        public ColorFilter ColorFilter { get; set; }
        public PenLineCap StrokeCap { get; set; }
        public PenLineJoin StrokeJoin { get; set; }
        public double  StrokeMiter { get; set; }
        public double  StrokeWidth { get; set; }
        public PathEffect PathEffect { get; set; }
        public PorterDuffXfermode Xfermode { get; set; }
        public Shader Shader { get; set; }
        public Typeface Typeface { get; set; }
        public double  TextSize { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    (ColorFilter as IDisposable)?.Dispose();
                    ColorFilter = null;

                    (PathEffect as IDisposable)?.Dispose();
                    PathEffect = null;

                    (Xfermode as IDisposable)?.Dispose();
                    Xfermode = null;

                    (Shader as IDisposable)?.Dispose();
                    Shader = null;
                }

                disposedValue = true;
            }
        }
    }
}