using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.Runtime.PairingRegistries
{
    /// <summary>
    /// 共通：親(parentId) -> stringValue（親から受け渡された値）
    /// 
    /// ここは「ビルダー基盤」ではなく、ただの SSOT（辞書）として使う。
    /// そのため OneLevelRegistryBase は継承しない（未来の混乱を避ける）。
    /// 
    /// メモ：
    /// - "stringValue" の意味はここでは決めない（ただの string）。
    /// - Secondary / Tertiary など複数系統から参照してOK。
    /// 
    /// 注意：スレッドセーフではない（通常のUI運用なら問題になりにくい想定）。
    /// </summary>
    public sealed class ParentPassedValueRegistry
    {
        private readonly Dictionary<string, string> _parentToValue = new();

        private static ParentPassedValueRegistry? _instance;
        public static ParentPassedValueRegistry Instance => _instance ??= new ParentPassedValueRegistry();
        private ParentPassedValueRegistry() { }

        /// <summary>
        /// 親(parentId)に対して、受け渡し値(stringValue)を保存（上書き）する。
        /// </summary>
        public void Set(string parentId, string stringValue)
        {
            if (string.IsNullOrEmpty(parentId)) return;
            if (stringValue == null) return; // 最小：null は入れない

            _parentToValue[parentId] = stringValue;
        }

        /// <summary>
        /// 親(parentId)に対して、受け渡し値(stringValue)を取得する。
        /// 無ければ false。
        /// </summary>
        public bool TryGet(string parentId, out string? stringValue)
        {
            stringValue = null;
            if (string.IsNullOrEmpty(parentId)) return false;

            if (_parentToValue.TryGetValue(parentId, out var v))
            {
                stringValue = v;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 親(parentId)の値を削除する。
        /// </summary>
        public bool RemoveValue(string parentId)
        {
            if (string.IsNullOrEmpty(parentId)) return false;
            return _parentToValue.Remove(parentId);
        }

        /// <summary>
        /// デバッグ/後片付け用：全消し。
        /// </summary>
        public void Clear()
        {
            _parentToValue.Clear();
        }
    }
}
