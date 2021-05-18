using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Avalonia.Lottie.Model;
using Avalonia.Lottie.Model.Layer;

namespace Avalonia.Lottie
{
    /// <summary>
    ///     After Effects/Bodymovin composition model. This is the serialized model from which the
    ///     animation will be created.
    ///     To create one, use <see cref="LottieCompositionFactory" />.
    ///     It can be used with a <seealso cref="LottieAnimationView" /> or
    ///     <seealso cref="Lottie" />.
    /// </summary>
    public class LottieComposition : IDisposable
    {
        private readonly PerformanceTracker _performanceTracker = new();
        private readonly HashSet<string> _warnings = new();
        private Dictionary<string, LottieImageAsset> _images;
        private Dictionary<long, Layer> _layerMap;
        private Dictionary<string, List<Layer>> _precomps;
        public bool Disposed { get; set; }

        /**
         * Map of font names to fonts
         */
        public virtual Dictionary<string, Font> Fonts { get; private set; }

        public virtual Dictionary<int, FontCharacter> Characters { get; private set; }
        public List<Layer> Layers { get; private set; }

        // This is stored as a set to avoid duplicates.
        public virtual Rect Bounds { get; private set; }
        public double  StartFrame { get; private set; }
        public double  EndFrame { get; private set; }
        public double  FrameRate { get; private set; }

        public List<string> Warnings => _warnings.ToList();

        public virtual bool PerformanceTrackingEnabled
        {
            get => _performanceTracker.Enabled;
            set => _performanceTracker.Enabled = value;
        }

        public virtual PerformanceTracker PerformanceTracker => _performanceTracker;

        public virtual double  Duration => (long) (DurationFrames / FrameRate * 1000);

        public virtual bool HasImages => _images.Count > 0;

        public virtual Dictionary<string, LottieImageAsset> Images => _images;

        internal virtual double  DurationFrames => EndFrame - StartFrame;

        public void Dispose()
        {
            Disposed = true;

            foreach (var item in _images)
            {
                item.Value.Bitmap.Dispose();
                item.Value.Bitmap = null;
            }

            _images.Clear();

            foreach (var item in _layerMap) item.Value.Dispose();
            _layerMap.Clear();

            foreach (var item in Layers) item.Dispose();
            Layers.Clear();
        }

        internal void AddWarning(string warning)
        {
            Debug.WriteLine(warning, LottieLog.Tag);
            _warnings.Add(warning);
        }

        internal virtual Layer LayerModelForId(long id)
        {
            _layerMap.TryGetValue(id, out var layer);
            return layer;
        }

        public void Init(Rect bounds, double  startFrame, double  endFrame, double  frameRate, List<Layer> layers,
            Dictionary<long, Layer> layerMap, Dictionary<string, List<Layer>> precomps,
            Dictionary<string, LottieImageAsset> images, Dictionary<int, FontCharacter> characters,
            Dictionary<string, Font> fonts)
        {
            Bounds = bounds;
            StartFrame = startFrame;
            EndFrame = endFrame;
            FrameRate = frameRate;
            Layers = layers;
            _layerMap = layerMap;
            _precomps = precomps;
            _images = images;
            Characters = characters;
            Fonts = fonts;
        }

        internal virtual List<Layer> GetPrecomps(string id)
        {
            return _precomps[id];
        }

        public override string ToString()
        {
            var sb = new StringBuilder("LottieComposition:\n");
            foreach (var layer in Layers) sb.Append(layer.ToString("\t"));
            return sb.ToString();
        }
    }
}