using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static global::ApplicationSuite.Runtime.Pairing.PairSyncRelay;

// FILE: Runtime/Pairing/PairRegistry.cs
// ROLE: 「書庫」— 親子ペアの同期状態を保存するだけのレジストリ本体（シングルトン）。
// NOTE: ロジックや通知は一切ここで行わない。純粋な CRUD のみ。
// DEP: SyncState は PairSyncRelay 側に定義される小型クラス（同一アセンブリ内参照）。

namespace ApplicationSuite.Runtime.Pairing
{
    /// <summary>
    /// 親子ペアの同期状態を一元保存する「書庫」。
    /// 役割は純粋な保存／取得／削除のみ。通知や上位ロジックは PairSyncRelay が担当。
    /// </summary>
    public class PairRegistry
    {
        // ---- Singleton ------------------------------------------------------

        private static PairRegistry _instance;
        public static PairRegistry Instance
        {
            get
            {
                if (_instance == null) _instance = new PairRegistry();
                return _instance;
            }
        }

        // Key = (parentId, childId)
        // parentId → childId と RealtimeSync フラグ
        private readonly Dictionary<(string, string), bool> _registry;


        // プライベートコンストラクタにより外部からの生成を防止
        private PairRegistry()
        {
            _registry = new();
        }

        // ---- CRUD -----------------------------------------------------------
        // 登録
        public void Register(string parentId, string childId, bool SyncFlig)
        {
            var key = (parentId, childId);
            _registry[key] = SyncFlig;            
        }
        // 取得
        public bool? GetSyncFlag(string parentId, string childId)
        {
            var key = (parentId, childId);
            if (_registry.TryGetValue(key, out var flag))
            {
                return flag;
            }
            return null; // 見つからない場合は null を返す
        }
        // 削除
        public void Unregister(string parentId, string childId)
        {
            var key = (parentId, childId);
            _registry.Remove(key);

        }
    }
}
