
using System;
using Avalonia;

namespace Avalonia.Lottie
{
    public static class RectExt
    {
        public static void Set(ref Rect rect, float left, float top, float right, float bottom)
        {
            rect = new Rect(left, top, Math.Abs(right - left), Math.Abs(bottom - top));
            //
            // rect.X = left;
            // rect.Y = top;
            // rect.Width = Math.Abs(right - left);
            // rect.Height = Math.Abs(bottom - top);
        }
        
        public static void Set(ref Rect rect, double left, double top, double right, double bottom)
        {
            rect = new Rect(left, top, Math.Abs(right - left), Math.Abs(bottom - top));
            //
            // rect.X = left;
            // rect.Y = top;
            // rect.Width = Math.Abs(right - left);
            // rect.Height = Math.Abs(bottom - top);
        }

        public static void Set(ref Rect rect, Rect newRect)
        {
            rect = new Rect(newRect.X, newRect.Y, newRect.Width, newRect.Height);
            //
            // rect.X = newRect.X;
            // rect.Y = newRect.Y;
            // rect.Width = newRect.Width;
            // rect.Height = newRect.Height;
        }
    }
}
