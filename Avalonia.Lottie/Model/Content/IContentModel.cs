using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Model.Layer;

namespace Avalonia.Lottie.Model.Content
{
    public interface IContentModel
    {
        IContent ToContent(LottieDrawable drawable, BaseLayer layer);
    }
}