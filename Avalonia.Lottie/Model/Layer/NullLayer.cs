using Avalonia.Lottie.Animation.Content;

namespace Avalonia.Lottie.Model.Layer
{
    internal class NullLayer : BaseLayer
    {
        internal NullLayer(Lottie lottie, Layer layerModel) : base(lottie, layerModel)
        {
        }

        public override void DrawLayer(BitmapCanvas canvas, Matrix3X3 parentMatrix, byte parentAlpha)
        {
            // Do nothing.
        }

        public override void GetBounds(ref Rect outBounds, Matrix3X3 parentMatrix)
        {
            base.GetBounds(ref outBounds, parentMatrix);
            RectExt.Set(ref outBounds, 0, 0, 0, 0);
        }
    }
}