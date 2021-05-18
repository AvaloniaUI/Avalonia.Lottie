using System.Numerics;

namespace Avalonia.Lottie.Model
{
    public class CubicCurveData
    {
        private Vector _controlPoint1;
        private Vector _controlPoint2;
        private Vector _vertex;

        internal CubicCurveData()
        {
            _controlPoint1 = new Vector();
            _controlPoint2 = new Vector();
            _vertex = new Vector();
        }

        internal CubicCurveData(Vector controlPoint1, Vector controlPoint2, Vector vertex)
        {
            _controlPoint1 = controlPoint1;
            _controlPoint2 = controlPoint2;
            _vertex = vertex;
        }

        internal virtual Vector ControlPoint1 => _controlPoint1;

        internal virtual Vector ControlPoint2 => _controlPoint2;

        internal virtual Vector Vertex => _vertex;

        internal virtual void SetControlPoint1(double x, double  y)
        {
            _controlPoint1 = new Vector(x, y);
        }

        internal virtual void SetControlPoint2(double x, double  y)
        {
            _controlPoint2 = new Vector(x, y);
 
        }

        internal virtual void SetVertex(double x, double  y)
        {
            _vertex = new Vector(x, y);
 
        }
    }
}