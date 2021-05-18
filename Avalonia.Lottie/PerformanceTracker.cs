using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Lottie.Utils;

namespace Avalonia.Lottie
{
    public class PerformanceTracker
    {
        private readonly IComparer<Tuple<string, double ?>> _floatComparator = new ComparatorAnonymousInnerClass();
        private readonly Dictionary<string, MeanCalculator> _layerRenderTimes = new();

        private bool _enabled;

        public virtual bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        public virtual List<Tuple<string, double ?>> SortedRenderTimes
        {
            get
            {
                if (!_enabled) return new List<Tuple<string, double ?>>();
                var sortedRenderTimes = new List<Tuple<string, double ?>>(_layerRenderTimes.Count);
                foreach (var e in _layerRenderTimes.SetOfKeyValuePairs())
                    sortedRenderTimes.Add(new Tuple<string, double ?>(e.Key, e.Value.Mean));
                sortedRenderTimes.Sort(_floatComparator);
                return sortedRenderTimes;
            }
        }

        public event EventHandler<FrameRenderedEventArgs> FrameRendered;

        public virtual void RecordRenderTime(string layerName, double  millis)
        {
            if (!_enabled) return;
            if (!_layerRenderTimes.TryGetValue(layerName, out var meanCalculator))
            {
                meanCalculator = new MeanCalculator();
                _layerRenderTimes[layerName] = meanCalculator;
            }

            meanCalculator.Add(millis);
            if (layerName.Equals("__container")) OnFrameRendered(new FrameRenderedEventArgs(millis));
        }

        public virtual void ClearRenderTimes()
        {
            _layerRenderTimes.Clear();
        }

        public virtual void LogRenderTimes()
        {
            if (!_enabled) return;
            var sortedRenderTimes = SortedRenderTimes;
            Debug.WriteLine("Render times:", LottieLog.Tag);
            for (var i = 0; i < sortedRenderTimes.Count; i++)
            {
                var layer = sortedRenderTimes[i];
                Debug.WriteLine(string.Format("\t\t{0,30}:{1:F2}", layer.Item1, layer.Item2), LottieLog.Tag);
            }
        }

        protected virtual void OnFrameRendered(FrameRenderedEventArgs e)
        {
            FrameRendered?.Invoke(this, e);
        }

        public class FrameRenderedEventArgs : EventArgs
        {
            public FrameRenderedEventArgs(double renderTimeMs)
            {
                RenderTimeMs = renderTimeMs;
            }

            public double  RenderTimeMs { get; }
        }

        private class ComparatorAnonymousInnerClass : IComparer<Tuple<string, double ?>>
        {
            public int Compare(Tuple<string, double ?> o1, Tuple<string, double ?> o2)
            {
                var r1 = o1.Item2;
                var r2 = o2.Item2;
                if (r2 > r1) return 1;
                if (r1 > r2) return -1;
                return 0;
            }
        }
    }
}