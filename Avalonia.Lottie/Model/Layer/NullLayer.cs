using Avalonia.Lottie.Animation.Content;

namespace Avalonia.Lottie.Model.Layer
{
    internal class NullLayer : BaseLayer
    {
        internal NullLayer(Lottie lottie, Layer layerModel) : base(lottie, layerModel)
        {
        }

        public override void DrawLayer(BitmapCanvas canvas, Matrix parentMatrix, byte parentAlpha)
        {
            // Do nothing.
        }

        public override void GetBounds(ref Rect outBounds, Matrix parentMatrix)
        {
            base.GetBounds(ref outBounds, parentMatrix);
            RectExt.Set(ref outBounds, 0, 0, 0, 0);
        }
    }
}