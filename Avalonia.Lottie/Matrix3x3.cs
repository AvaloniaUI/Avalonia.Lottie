using System.Numerics;

namespace Avalonia.Lottie
{
    // public class Matrix
    // {
    //     public double  M11;
    //     public double  M12;
    //     public double  M13;
    //     public double  M21;
    //     public double  M22;
    //     public double  M23;
    //     public double  M31;
    //     public double  M32;
    //     public double  M33;
    //
    //     public static Matrix CreateIdentity()
    //     {
    //         return new()
    //         {
    //             M11 = 1,
    //             M12 = 0,
    //             M13 = 0,
    //             M21 = 0,
    //             M22 = 1,
    //             M23 = 0,
    //             M31 = 0,
    //             M32 = 0,
    //             M33 = 1
    //         };
    //     }
    //
    //     public void Set(Matrix m)
    //     {
    //         M11 = m.M11;
    //         M12 = m.M12;
    //         M13 = m.M13;
    //         M21 = m.M21;
    //         M22 = m.M22;
    //         M23 = m.M23;
    //         M31 = m.M31;
    //         M32 = m.M32;
    //         M33 = m.M33;
    //     }
    //
    //     public void Reset()
    //     {
    //         M11 = 1;
    //         M12 = 0;
    //         M13 = 0;
    //         M21 = 0;
    //         M22 = 1;
    //         M23 = 0;
    //         M31 = 0;
    //         M32 = 0;
    //         M33 = 1;
    //     }
    //
    //     public static Matrix operator *(Matrix m1, Matrix m2)
    //     {
    //         return new()
    //         {
    //             M11 = m1.M11 * m2.M11 + m1.M12 * m2.M21 + m1.M13 * m2.M31,
    //             M12 = m1.M11 * m2.M12 + m1.M12 * m2.M22 + m1.M13 * m2.M32,
    //             M13 = m1.M11 * m2.M13 + m1.M12 * m2.M23 + m1.M13 * m2.M33,
    //             M21 = m1.M21 * m2.M11 + m1.M22 * m2.M21 + m1.M23 * m2.M31,
    //             M22 = m1.M21 * m2.M12 + m1.M22 * m2.M22 + m1.M23 * m2.M32,
    //             M23 = m1.M21 * m2.M13 + m1.M22 * m2.M23 + m1.M23 * m2.M33,
    //             M31 = m1.M31 * m2.M11 + m1.M32 * m2.M21 + m1.M33 * m2.M31,
    //             M32 = m1.M31 * m2.M12 + m1.M32 * m2.M22 + m1.M33 * m2.M32,
    //             M33 = m1.M31 * m2.M13 + m1.M32 * m2.M23 + m1.M33 * m2.M33
    //         };
    //     }
    //
    //     public Vector Transform(Vector v)
    //     {
    //         return new(
    //             (float) (v.X * M11 + v.Y * M12 + M13),
    //             (float) (v.X * M21 + v.Y * M22 + M23));
    //     }
    // }
}