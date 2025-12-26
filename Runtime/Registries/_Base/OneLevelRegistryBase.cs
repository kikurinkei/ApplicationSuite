using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.Runtime.Registries._Base
{
    public abstract class OneLevelRegistryBase<TValue>
    {
        protected readonly Dictionary<string, TValue> _items = new();

        public void Register(string windowUniqueId, TValue value)
        {
            _items[windowUniqueId] = value;
            Console.WriteLine($"[Register] {typeof(TValue).Name}: Window=[{windowUniqueId}]");
        }

        public TValue? Get(string windowUniqueId)
        {
            if (_items.TryGetValue(windowUniqueId, out var value))
                return value;
            return default;
        }

        public T? Get<T>(string windowUniqueId) where T : class
        {
            return Get(windowUniqueId) as T;
        }

        public bool Remove(string windowUniqueId)
        {
            if (_items.Remove(windowUniqueId))
            {
                Console.WriteLine($"[Remove] {typeof(TValue).Name}: Window=[{windowUniqueId}]");
                return true;
            }
            return false;
        }
        public List<string> GetKeys()
        {
            return new List<string>(_items.Keys);
        }

        public bool Clear()
        {
            _items.Clear();
            Console.WriteLine($"[ClearAll] {typeof(TValue).Name}: All windows cleared.");
            return true;
        }
    }
    /// <summary>
    /// OneLevelRegistryBase に対する拡張：空判定
    /// </summary>
    public static class OneLevelRegistryExtensions
    {
        public static bool IsEmpty<T>(this OneLevelRegistryBase<T> registry)
        {
            var keys = registry.GetKeys();
            return keys == null || keys.Count == 0;
        }
    }
}