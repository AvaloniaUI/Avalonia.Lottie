using System.Collections.Generic;

namespace Avalonia.Lottie.Model
{
    public class LruCache<K, V>
    {
        private readonly int _capacity;
        private readonly Dictionary<K, LinkedListNode<LruCacheItem<K, V>>> _cacheMap = new();
        private readonly LinkedList<LruCacheItem<K, V>> _lruList = new();

        public LruCache(int capacity)
        {
            _capacity = capacity;
        }

        public V Get(K key)
        {
            lock (this)
            {
                if (_cacheMap.TryGetValue(key, out var node))
                {
                    var value = node.Value.Value;
                    _lruList.Remove(node);
                    _lruList.AddLast(node);
                    return value;
                }

                return default;
            }
        }

        public void Put(K key, V val)
        {
            lock (this)
            {
                if (_cacheMap.Count >= _capacity) RemoveFirst();

                var cacheItem = new LruCacheItem<K, V>(key, val);
                var node = new LinkedListNode<LruCacheItem<K, V>>(cacheItem);
                _lruList.AddLast(node);
                _cacheMap[key] = node;
            }
        }

        private void RemoveFirst()
        {
            // Remove from LRUPriority
            var node = _lruList.First;
            _lruList.RemoveFirst();

            // Remove from cache
            _cacheMap.Remove(node.Value.Key);
        }
    }

    internal class LruCacheItem<K, V>
    {
        public K Key;
        public V Value;

        public LruCacheItem(K k, V v)
        {
            Key = k;
            Value = v;
        }
    }
}