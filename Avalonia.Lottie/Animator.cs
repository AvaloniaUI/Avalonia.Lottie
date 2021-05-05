using Avalonia.Metadata;

[assembly: XmlnsDefinition("https://github.com/avaloniaui", "Avalonia.Lottie")]

namespace Avalonia.Lottie
{
    public abstract class Animator
    {
        public virtual long Duration { get; set; }

        public abstract bool IsRunning { get; }

        public virtual void Cancel()
        {
            AnimationCanceled();
        }

        protected virtual void AnimationCanceled()
        {
        }
    }
}