using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.Runtime.Registries._Base
{
    /// <summary>
    /// 3階層構造（Shell → Utility → Element）で T を保持する汎用レジストリ基底クラス。
    /// </summary>
    public abstract class ThreeLevelRegistryBase<T>
    {
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, T>>> _registry
            = new();

        /// <summary>
        /// 登録：Shell → Utility → Element に T を格納。
        /// </summary>
        public void Register(string shellId, string utilityId, string elementId, T value)
        {
            if (!_registry.TryGetValue(shellId, out var utilityDict))
            {
                utilityDict = new Dictionary<string, Dictionary<string, T>>();
                _registry[shellId] = utilityDict;
            }

            if (!utilityDict.TryGetValue(utilityId, out var elementDict))
            {
                elementDict = new Dictionary<string, T>();
                utilityDict[utilityId] = elementDict;
            }

            elementDict[elementId] = value;
        }

        /// <summary>
        /// 取得：指定されたキーに一致する T を取得。
        /// </summary>
        public bool TryGet(string shellId, string utilityId, string elementId, out T value)
        {
            value = default!;
            return _registry.TryGetValue(shellId, out var utilityDict)
                && utilityDict.TryGetValue(utilityId, out var elementDict)
                && elementDict.TryGetValue(elementId, out value);
        }

        /// <summary>
        /// 全データをクリア。
        /// </summary>
        public void Clear()
        {
            _registry.Clear();
        }
    }
}
