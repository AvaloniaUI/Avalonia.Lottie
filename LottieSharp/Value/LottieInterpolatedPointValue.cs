
/* Unmerged change from project 'LottieSharp (netcoreapp3.0)'
Before:

using LottieSharp.Utils;
After:
using LottieSharp.Utils;

*/

using System.Numerics;
using LottieSharp.Utils;


namespace LottieSharp.Value
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

        protected override Vector2 InterpolateValue(Vector2 startValue, Vector2 endValue, float progress)
        {
            _point.X = MiscUtils.Lerp(startValue.X, endValue.X, progress);
            _point.Y = MiscUtils.Lerp(startValue.Y, endValue.Y, progress);
            return _point;
        }
    }
}
