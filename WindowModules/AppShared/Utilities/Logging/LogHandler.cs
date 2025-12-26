using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.AppShared.Utilities.Logging
{
    /// <summary>
    /// LogHandler
    /// ログ記録の集中管理を担う静的クラス。
    /// ViewModelやユーティリティ層から呼び出され、LogRegistryにログを追加する。
    /// 設計思想：
    /// - ログ生成責務を分離し、記録処理を一元化
    /// - 拡張：ログレベルや出力先の追加余地あり
    /// </summary>
    public static class LogHandler
    {
        public static void Handle(string className, string identifier, string category, string message)
        {
            var entry = new LogEntry(className, identifier, category, message);
            LogRegistry.Instance.Add(entry);
        }
    }
}
