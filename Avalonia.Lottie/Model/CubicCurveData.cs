using System.Numerics;

namespace Avalonia.Lottie.Model
{
    public class CubicCurveData
    {
        private Vector2 _controlPoint1;
        private Vector2 _controlPoint2;
        private Vector2 _vertex;

        internal CubicCurveData()
        {
            _controlPoint1 = new Vector2();
            _controlPoint2 = new Vector2();
            _vertex = new Vector2();
        }

        internal CubicCurveData(Vector2 controlPoint1, Vector2 controlPoint2, Vector2 vertex)
        {
            _controlPoint1 = controlPoint1;
            _controlPoint2 = controlPoint2;
            _vertex = vertex;
        }

        internal virtual Vector2 ControlPoint1 => _controlPoint1;

        internal virtual Vector2 ControlPoint2 => _controlPoint2;

        internal virtual Vector2 Vertex => _vertex;

        internal virtual void SetControlPoint1(double x, double  y)
        {
            _controlPoint1.X = (float)x;
            _controlPoint1.Y =(float) y;
        }

        internal virtual void SetControlPoint2(double x, double  y)
        {
            _controlPoint2.X = (float)x;
            _controlPoint2.Y = (float)y;
        }

        internal virtual void SetVertex(double x, double  y)
        {
            _vertex.X = (float)x;
            _vertex.Y = (float)y;
        }
    }
}