/* Unmerged change from project 'Avalonia.Lottie (netcoreapp3.0)'
Before:

using Avalonia.Lottie.Utils;
After:
using Avalonia.Lottie.Utils;

*/

using System.Numerics;
using Avalonia.Lottie.Utils;

namespace Avalonia.Lottie.Value
{
    // ReSharper disable once UnusedMember.Global
    public class LottieInterpolatedPointValue : LottieInterpolatedValue<Vector>
    {
        private Vector _point;

        public LottieInterpolatedPointValue(Vector startValue, Vector endValue)
            : base(startValue, endValue)
        {
        }

        public LottieInterpolatedPointValue(Vector startValue, Vector endValue, IInterpolator interpolator)
            : base(startValue, endValue, interpolator)
        {
        }

        protected override Vector InterpolateValue(Vector startValue, Vector endValue, double  progress)
        { 
            _point =  new Vector(MiscUtils.Lerp(startValue.X, endValue.X, progress),
              MiscUtils.Lerp(startValue.Y, endValue.Y, progress));
            return _point;
        }
    }
}