﻿namespace Avalonia.Lottie.Model
{
    internal class LottieCompositionCache
    {
        //private static readonly int _cacheSizeMB = 10;
        private static readonly int _cacheSizeCount = 10;

        private readonly LruCache<string, LottieComposition>
            _cache = new(_cacheSizeCount); //1024 * 1024 * _cacheSizeMB);

        public static LottieCompositionCache Instance { get; } = new();

        public LottieComposition GetRawRes(int rawRes)
        {
            return Get(rawRes.ToString());
        }

        public LottieComposition Get(string assetName)
        {
            var layer = _cache.Get(assetName);
            if (layer?.Disposed ?? true)
                return null;
            return layer;
        }

        public void Put(int rawRes, LottieComposition composition)
        {
            Put(rawRes.ToString(), composition);
        }

        public void Put(string cacheKey, LottieComposition composition)
        {
            if (cacheKey == null) return;
            _cache.Put(cacheKey, composition);
        }
    }
}