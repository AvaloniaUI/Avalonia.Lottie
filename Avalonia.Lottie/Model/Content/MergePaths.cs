using System.Diagnostics;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Model.Layer;

namespace Avalonia.Lottie.Model.Content
{
    public class MergePaths : IContentModel
    {
        public enum MergePathsMode
        {
            Merge = 1,
            Add = 2,
            Subtract = 3,
            Intersect = 4,
            ExcludeIntersections = 5
        }

        private readonly MergePathsMode _mode;

        public MergePaths(string name, MergePathsMode mode)
        {
            Name = name;
            _mode = mode;
        }

        public virtual string Name { get; }

        internal virtual MergePathsMode Mode => _mode;

        public IContent ToContent(Lottie drawable, BaseLayer layer)
        {
            return new MergePathsContent(this);
        }

        public override string ToString()
        {
            return "MergePaths{" + "mode=" + _mode + '}';
        }
    }
}