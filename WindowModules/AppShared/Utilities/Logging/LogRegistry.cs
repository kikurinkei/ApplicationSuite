using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.AppShared.Utilities.Logging
{
    /// <summary>
    /// LogRegistry
    /// ログの永続保持と取得を担うレジストリクラス。
    /// 設計思想：
    /// - 単一インスタンスによる集中管理（Singleton）
    /// - 拡張：フィルタ取得・削除・永続化などの追加余地あり
    /// </summary>
    public class LogRegistry
    {
        private static readonly LogRegistry _instance = new LogRegistry();
        public static LogRegistry Instance => _instance;

        private readonly List<LogEntry> _logs = new();

        // ログ追加
        public void Add(LogEntry entry)
        {
            _logs.Add(entry);
        }

        // 全ログ取得（コピーを返す）
        public List<LogEntry> GetAll()
        {
            return new List<LogEntry>(_logs);
        }
    }
}
