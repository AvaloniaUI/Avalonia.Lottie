using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia.Lottie.Utils;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Parser
{
    internal static class KeyframeParser
    {
        /// <summary>
        ///     Some animations get exported with insane cp values in the tens of thousands.
        ///     PathInterpolator fails to create the interpolator in those cases and hangs.
        ///     Clamping the cp helps prevent that.
        /// </summary>
        private const double MaxCpValue = 100;

        private static readonly IInterpolator LinearInterpolator = new LinearInterpolator();

        private static readonly object Lock = new();
        private static Dictionary<int, WeakReference<IInterpolator>> _pathInterpolatorCache;

        // https://github.com/airbnb/lottie-android/issues/464 
        private static Dictionary<int, WeakReference<IInterpolator>> PathInterpolatorCache()
        {
            return _pathInterpolatorCache ??
                   (_pathInterpolatorCache = new Dictionary<int, WeakReference<IInterpolator>>());
        }

        private static bool GetInterpolator(int hash, out WeakReference<IInterpolator> interpolatorRef)
        {
            // This must be synchronized because get and put isn't thread safe because 
            // SparseArrayCompat has to create new sized arrays sometimes. 
            lock (Lock)
            {
                return PathInterpolatorCache().TryGetValue(hash, out interpolatorRef);
            }
        }

        private static void PutInterpolator(int hash, WeakReference<IInterpolator> interpolator)
        {
            // This must be synchronized because get and put isn't thread safe because 
            // SparseArrayCompat has to create new sized arrays sometimes. 
            lock (Lock)
            {
                _pathInterpolatorCache[hash] = interpolator;
            }
        }

        internal static Keyframe<T> Parse<T>(JsonReader reader, LottieComposition composition,
            IValueParser<T> valueParser, bool animated)
        {
            if (animated) return ParseKeyframe(composition, reader, valueParser);
            return ParseStaticValue(reader, valueParser);
        }

        /// <summary>
        ///     beginObject will already be called on the keyframe so it can be differentiated with
        ///     a non animated value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="composition"></param>
        /// <param name="reader"></param>
        /// <param name="scale"></param>
        /// <param name="valueParser"></param>
        /// <returns></returns>
        private static Keyframe<T> ParseKeyframe<T>(LottieComposition composition, JsonReader reader,
            IValueParser<T> valueParser)
        {
            Vector? cp1 = null;
            Vector? cp2 = null;
            double startFrame = 0;
            var startValue = default(T);
            var endValue = default(T);
            var hold = false;
            IInterpolator interpolator;

            // Only used by PathKeyframe 
            Vector? pathCp1 = null;
            Vector? pathCp2 = null;

            reader.BeginObject();
            while (reader.HasNext())
                switch (reader.NextName())
                {
                    case "t":
                        startFrame = reader.NextDouble();
                        break;
                    case "s":
                        startValue = valueParser.Parse(reader);
                        break;
                    case "e":
                        endValue = valueParser.Parse(reader);
                        break;
                    case "o":
                        cp1 = JsonUtils.JsonToPoint(reader);
                        break;
                    case "i":
                        cp2 = JsonUtils.JsonToPoint(reader);
                        break;
                    case "h":
                        hold = reader.NextInt() == 1;
                        break;
                    case "to":
                        pathCp1 = JsonUtils.JsonToPoint(reader);
                        break;
                    case "ti":
                        pathCp2 = JsonUtils.JsonToPoint(reader);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }

            reader.EndObject();

            if (hold)
            {
                endValue = startValue;
                // TODO: create a HoldInterpolator so progress changes don't invalidate. 
                interpolator = LinearInterpolator;
            }
            else if (cp1 != null && cp2 != null)
            {
                var hash = Utils.Utils.HashFor(cp1.Value.X, cp1.Value.Y, cp2.Value.X, cp2.Value.Y);
                if (GetInterpolator(hash, out var interpolatorRef) == false ||
                    interpolatorRef.TryGetTarget(out interpolator) == false)
                {
                    interpolator = new PathInterpolator(cp1.Value.X, cp1.Value.Y, cp2.Value.X,
                        cp2.Value.Y);
                    try
                    {
                        PutInterpolator(hash, new WeakReference<IInterpolator>(interpolator));
                    }
                    catch
                    {
                        // It is not clear why but SparseArrayCompat sometimes fails with this: 
                        //     https://github.com/airbnb/lottie-android/issues/452 
                        // Because this is not a critical operation, we can safely just ignore it. 
                        // I was unable to repro this to attempt a proper fix. 
                    }
                }
            }
            else
            {
                interpolator = LinearInterpolator;
            }

            var keyframe = new Keyframe<T>(composition, startValue, endValue, interpolator, startFrame, null)
            {
                PathCp1 = pathCp1,
                PathCp2 = pathCp2
            };
            return keyframe;
        }

        private static Keyframe<T> ParseStaticValue<T>(JsonReader reader, IValueParser<T> valueParser)
        {
            var value = valueParser.Parse(reader);
            return new Keyframe<T>(value);
        }
    }
}