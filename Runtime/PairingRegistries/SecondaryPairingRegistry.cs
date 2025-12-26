using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.Runtime.PairingRegistries
{
    /// <summary>
    /// Secondary 用：親(parentId) ⇔ 子(secondaryWindowUniqueId) のペアを保持する書庫。
    /// 
    /// ここは「ビルダー基盤」ではなく、ただの SSOT（辞書）として使う。
    /// そのため OneLevelRegistryBase は継承しない（未来の混乱を避ける）。
    /// 
    /// 仕様（最小）：
    /// - parentId 1つに対して、Secondary は1つだけ（上書き）
    /// - 逆引き（childId -> parentId）は全走査
    ///   ※必要になったら逆引き辞書を追加
    /// 
    /// 注意：スレッドセーフではない（通常のUI運用なら問題になりにくい想定）。
    /// </summary>
    public sealed class SecondaryPairingRegistry
    {
        private readonly Dictionary<string, string> _parentToChild = new();

        private static SecondaryPairingRegistry? _instance;
        public static SecondaryPairingRegistry Instance => _instance ??= new SecondaryPairingRegistry();
        private SecondaryPairingRegistry() { }

        /// <summary>
        /// parentId に対して Secondary の childId を登録する。
        /// 既に登録があれば上書き。
        /// </summary>
        public void RegisterPair(string parentId, string secondaryWindowUniqueId)
        {
            if (string.IsNullOrEmpty(parentId)) return;
            if (string.IsNullOrEmpty(secondaryWindowUniqueId)) return;

            _parentToChild[parentId] = secondaryWindowUniqueId;
        }

        /// <summary>
        /// parentId に紐づく Secondary の childId を取得する。
        /// 無ければ false。
        /// </summary>
        public bool TryGetChild(string parentId, out string? secondaryWindowUniqueId)
        {
            secondaryWindowUniqueId = null;
            if (string.IsNullOrEmpty(parentId)) return false;

            if (_parentToChild.TryGetValue(parentId, out var v))
            {
                secondaryWindowUniqueId = v;
                return true;
            }

            return false;
        }

        /// <summary>
        /// parentId を指定してペア解除する。
        /// </summary>
        public bool UnregisterByParent(string parentId)
        {
            if (string.IsNullOrEmpty(parentId)) return false;
            return _parentToChild.Remove(parentId);
        }

        /// <summary>
        /// childId（Secondary側）から parentId を探す（最小：全走査）。
        /// </summary>
        public bool TryGetParentByChild(string secondaryWindowUniqueId, out string? parentId)
        {
            parentId = null;
            if (string.IsNullOrEmpty(secondaryWindowUniqueId)) return false;

            foreach (var kv in _parentToChild)
            {
                if (kv.Value == secondaryWindowUniqueId)
                {
                    parentId = kv.Key;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// childId を指定してペア解除する。
        /// </summary>
        public bool UnregisterByChild(string secondaryWindowUniqueId)
        {
            if (string.IsNullOrEmpty(secondaryWindowUniqueId)) return false;

            if (!TryGetParentByChild(secondaryWindowUniqueId, out var parentId)) return false;
            if (string.IsNullOrEmpty(parentId)) return false;

            return _parentToChild.Remove(parentId);
        }

        /// <summary>
        /// デバッグ/後片付け用：全消し。
        /// </summary>
        public void Clear()
        {
            _parentToChild.Clear();
        }
    }
}

