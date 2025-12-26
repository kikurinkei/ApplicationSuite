using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// FILE: Runtime/Pairing/PairingRegistry.cs
// ROLE: 「書庫」— parentId ごとの状態（ChildId / LatestElementId）を保存するだけ（Singleton）。
// NOTE: ロジックや通知は一切ここで行わない。純粋な CRUD のみ。
//       （配達する/しない等の判断は PairingHub / Relay 側でやる）

namespace ApplicationSuite.Runtime.Registries
{
    /// <summary>
    /// 親(parentId)ごとの同期状態を一元保存する「書庫」。
    /// - ChildId（紐づく子ウインドウID）
    /// - LatestElementId（最新の選択ID）
    /// を保持するだけ。
    /// </summary>
    public class PairingRegistry
    {
        // ---- Singleton ------------------------------------------------------

        private static PairingRegistry _instance;
        public static PairingRegistry Instance
        {
            get
            {
                if (_instance == null) _instance = new PairingRegistry();
                return _instance;
            }
        }

        // ---- Storage（辞書は1つだけ） -------------------------------------

        // Key = parentId
        // Value = PairingState（ChildId / LatestElementId の2つだけ）
        private readonly Dictionary<string, PairingState> _registry;

        // 外部から new させない
        private PairingRegistry()
        {
            _registry = new Dictionary<string, PairingState>();
        }

        // ---- CRUD（最低限） ------------------------------------------------

        // latest（最新の選択）を保存（上書き）
        public void SetLatest(string parentId, string elementId)
        {
            // state が無ければ作る
            PairingState state;
            if (!_registry.TryGetValue(parentId, out state))
            {
                state = new PairingState();
                _registry[parentId] = state;
            }

            state.LatestElementId = elementId;
        }

        // child（子ウインドウ）を登録（上書き）
        public void Register(string parentId, string childId)
        {
            // state が無ければ作る
            PairingState state;
            if (!_registry.TryGetValue(parentId, out state))
            {
                state = new PairingState();
                _registry[parentId] = state;
            }

            state.ChildId = childId;
        }

        // child を解除（childId 起点）
        // ※逆引き辞書は持たないので、parent を線形探索する
        public void UnregisterByChild(string childId)
        {
            string hitParentId = null;

            foreach (KeyValuePair<string, PairingState> kv in _registry)
            {
                if (kv.Value.ChildId == childId)
                {
                    hitParentId = kv.Key;
                    break;
                }
            }

            if (hitParentId == null) return;

            // child だけ外す（latest は残す）
            _registry[hitParentId].ChildId = null;

            // state が空なら掃除してもよい（ここでは掃除する）
            if (_registry[hitParentId].ChildId == null && _registry[hitParentId].LatestElementId == null)
            {
                _registry.Remove(hitParentId);
            }
        }

        // parent close：その parent の状態を丸ごと削除（latest も child も消える）
        public void UnregisterParent(string parentId)
        {
            _registry.Remove(parentId);
        }

        // ---- Get（最低限） -------------------------------------------------

        // 取得：latest
        public string GetLatestOrNull(string parentId)
        {
            PairingState state;
            if (_registry.TryGetValue(parentId, out state))
            {
                return state.LatestElementId;
            }
            return null;
        }

        // 取得：child
        public string GetChildOrNull(string parentId)
        {
            PairingState state;
            if (_registry.TryGetValue(parentId, out state))
            {
                return state.ChildId;
            }
            return null;
        }

        // ---- 内部クラス（最小） --------------------------------------------

        /// <summary>
        /// 1 parent あたりの状態（最小）。
        /// </summary>
        private class PairingState
        {
            public string ChildId;         // null 許容
            public string LatestElementId; // null 許容
        }
    }
}
