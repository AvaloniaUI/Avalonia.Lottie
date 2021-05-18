using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using Avalonia.Lottie.Utils;

namespace Avalonia.Lottie
{
    public static class MatrixExt
    {
        
        
        public static Vector Transform(this  Matrix m, Vector v)
        {
            return new Point(v.X, v.Y) * m;
        }
        
        public static Matrix PreConcat(Matrix matrix, Matrix transformAnimationMatrix)
        {
            return  transformAnimationMatrix * matrix ;
        }

        public static Matrix PreTranslate(Matrix matrix, double  dx, double  dy)
        {
            return Matrix.CreateTranslation(dx, dy) * matrix;
        }
 
        public static Matrix PreRotate(Matrix matrix, double  rotation)
        {
            var angle = Matrix.ToRadians(rotation);
 
            return  Matrix.CreateRotation(angle) * matrix;
        }

        public static Matrix PreRotate(Matrix matrix, double  rotation, double  px, double  py)
        {
            var angle = MathExt.ToRadians(rotation); 

            var tmp = Matrix.CreateTranslation(-px, -py) * Matrix.CreateRotation(angle) * Matrix.CreateTranslation(px, py);

            return tmp * matrix ;
        }
 
        public static Matrix PreScale(Matrix matrix, double  scaleX, double  scaleY)
        {
            return  Matrix.CreateScale(scaleX, scaleY) * matrix;
        }
  
        public static void MapRect(this Matrix matrix, ref Rect rect)
        {
            var p1 = new Vector((float) rect.Left, (float) rect.Top);
            var p2 = new Vector((float) rect.Right, (float) rect.Top);
            var p3 = new Vector((float) rect.Left, (float) rect.Bottom);
            var p4 = new Vector((float) rect.Right, (float) rect.Bottom);

            p1 = matrix.Transform(p1);
            p2 = matrix.Transform(p2);
            p3 = matrix.Transform(p3);
            p4 = matrix.Transform(p4);

            var xMin = Math.Min(Math.Min(Math.Min(p1.X, p2.X), p3.X), p4.X);
            var xMax = Math.Max(Math.Max(Math.Max(p1.X, p2.X), p3.X), p4.X);
            var yMax = Math.Max(Math.Max(Math.Max(p1.Y, p2.Y), p3.Y), p4.Y);
            var yMin = Math.Min(Math.Min(Math.Min(p1.Y, p2.Y), p3.Y), p4.Y);

            RectExt.Set(ref rect, new Rect(xMin, yMax, xMax, yMin));
        }

        public static void MapPoints(this Matrix matrix, ref Vector[] points)
        {
            for (var i = 0; i < points.Length; i++) points[i] = matrix.Transform(points[i]);
        }

        public static IEnumerable<IEnumerable<T>> Partition<T>
            (this IEnumerable<T> source, int size)
        {
            T[] array = null;
            var count = 0;
            foreach (var item in source)
            {
                if (array == null) array = new T[size];
                array[count] = item;
                count++;
                if (count == size)
                {
                    yield return new ReadOnlyCollection<T>(array);
                    array = null;
                    count = 0;
                }
            }

            if (array != null)
            {
                Array.Resize(ref array, count);
                yield return new ReadOnlyCollection<T>(array);
            }
        }
    }
}