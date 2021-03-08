using System.Collections.Generic;

namespace Avalonia.Lottie.Animation.Content
{
    public interface IContent
    {
        string Name { get; }

        void SetContents(List<IContent> contentsBefore, List<IContent> contentsAfter);
    }
}