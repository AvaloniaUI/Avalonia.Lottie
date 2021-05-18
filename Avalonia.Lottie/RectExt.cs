using System;

namespace Avalonia.Lottie
{
    public static class RectExt
    {
        public static void Set(ref Rect rect, double  left, double  top, double  right, double  bottom)
        {
            rect = new Rect(left, top, Math.Abs(right - left), Math.Abs(bottom - top));
          
        }
 

        public static void Set(ref Rect rect, Rect newRect)
        {
            rect = new Rect(newRect.X, newRect.Y, newRect.Width, newRect.Height);
           
        }
    }
}