namespace Avalonia.Lottie
{
    public class PorterDuffXfermode
    {
        public PorterDuffXfermode(PorterDuff.Mode mode)
        {
            Mode = mode;
        }

        public PorterDuff.Mode Mode { get; }
    }
}