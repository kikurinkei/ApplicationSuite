using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.Runtime.Registries._Base
{
    public abstract class TwoLevelRegistryBase<TValue>
    {
        protected readonly Dictionary<string, Dictionary<string, TValue>> _items = new();

        public void Register(string windowUniqueId, string elementId, TValue value)
        {
            if (!_items.ContainsKey(windowUniqueId))
                _items[windowUniqueId] = new();

            _items[windowUniqueId][elementId] = value;
            Console.WriteLine($"[Register] {typeof(TValue).Name}: Window=[{windowUniqueId}], Element=[{elementId}]");
        }

        public TValue? Get(string windowUniqueId, string elementId)
        {
            if (_items.TryGetValue(windowUniqueId, out var elementDict) &&
                elementDict.TryGetValue(elementId, out var value))
                return value;

            return default;
        }

        public T? Get<T>(string windowUniqueId, string elementId) where T : class
        {
            return Get(windowUniqueId, elementId) as T;
        }

        public bool Remove(string windowUniqueId, string elementId)
        {
            if (_items.TryGetValue(windowUniqueId, out var elementDict) && elementDict.Remove(elementId))
            {
                Console.WriteLine($"[Remove] {typeof(TValue).Name}: Window=[{windowUniqueId}], Element=[{elementId}]");
                if (elementDict.Count == 0)
                {
                    _items.Remove(windowUniqueId);
                    Console.WriteLine($"[Remove] All elements removed for Window=[{windowUniqueId}]");
                }
                return true;
            }
            return false;
        }

        public bool RemoveAll(string windowUniqueId)
        {
            if (_items.ContainsKey(windowUniqueId))
            {
                _items.Remove(windowUniqueId);
                Console.WriteLine($"[RemoveAll] {typeof(TValue).Name}: Window=[{windowUniqueId}]");
                return true;
            }
            return false;
        }

        public List<string> GetKeys(string windowUniqueId)
        {
            if (_items.TryGetValue(windowUniqueId, out var elementDict))
                return new List<string>(elementDict.Keys);

            return new List<string>();
        }

        public List<string> GetAllKeys()
        {
            return new List<string>(_items.Keys);
        }
    }
    /// <summary>
    /// TwoLevelRegistryBase に対する拡張：空判定
    /// </summary>
    public static class TwoLevelRegistryExtensions
    {
        public static bool IsEmpty<T>(this TwoLevelRegistryBase<T> registry)
        {
            var keys = registry.GetAllKeys();
            return keys == null || keys.Count == 0;
        }
    }
}