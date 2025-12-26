using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ApplicationSuite.Runtime.Service
{
    /// <summary>
    /// PrimaryWindowのCurrentContentViewModel（ElementId）を一元管理するシングルトン・クラスです。
    /// </summary>
    /// ParentWindowElementIdCache
    public class ParentWindowElementIdCache
    {
        // シングルトン・インスタンスの内部フィールド
        private static ParentWindowElementIdCache? _instance;

        /// <summary>
        /// WindowRegistry のシングルトン・インスタンス（遅延初期化）。
        /// </summary>
        public static ParentWindowElementIdCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ParentWindowElementIdCache();
                }
                return _instance;
            }
        }

        // 辞書によって各TransferMapServiceを管理します
        private Dictionary<string, string> _map;

        // プライベートコンストラクタにより外部からの生成を防止
        private ParentWindowElementIdCache()
        {
            _map = new();
        }
        public void AddOrUpdate(string windowId, string elementId)
        {
            _map[windowId] = elementId;
        }
        public void Remove(string windowId)
        {
            _map.Remove(windowId);
        }

        public string? Get(string windowId)
        {
            return _map.TryGetValue(windowId, out var value) ? value : null;
        }
    }
}
