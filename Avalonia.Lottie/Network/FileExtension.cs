using System.Diagnostics;

namespace Avalonia.Lottie.Network
{
    /// <summary>
    ///     Helpers for known Lottie file types.
    /// </summary>
    public class FileExtension
    {
        public static FileExtension Json = new(".json");
        public static FileExtension Zip = new(".zip");

        private FileExtension(string extension)
        {
            Extension = extension;
        }

        public string Extension { get; }

        public string TempExtension => ".temp" + Extension;

        public override string ToString()
        {
            return Extension;
        }

        public static FileExtension ForFile(string filename)
        {
            foreach (var e in new[] {Json, Zip})
                if (filename.EndsWith(e.Extension))
                    return e;
            // Default to Json.
            Debug.WriteLine("Unable to find correct extension for " + filename, LottieLog.Tag);
            return Json;
        }
    }
}