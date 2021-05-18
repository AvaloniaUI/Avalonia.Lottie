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
    public class LottieInterpolatedPointValue : LottieInterpolatedValue<Vector2>
    {
        private Vector2 _point;

        public LottieInterpolatedPointValue(Vector2 startValue, Vector2 endValue)
            : base(startValue, endValue)
        {
        }

        public LottieInterpolatedPointValue(Vector2 startValue, Vector2 endValue, IInterpolator interpolator)
            : base(startValue, endValue, interpolator)
        {
        }

        protected override Vector2 InterpolateValue(Vector2 startValue, Vector2 endValue, double  progress)
        {
            _point.X = (float) MiscUtils.Lerp(startValue.X, endValue.X, progress);
            _point.Y = (float) MiscUtils.Lerp(startValue.Y, endValue.Y, progress);
            return _point;
        }
    }
}