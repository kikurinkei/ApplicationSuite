using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.AppShared.Utilities.Logging
{
    /// <summary>
    /// 単一のログエントリを表すデータ構造。
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; }

        public string ClassName { get; } // 処理しているクラス名（ハードコーディングOK）

        public string Identifier { get; } // 任意の識別子（$"{hogehoge}{n}" など）

        public string Category { get; } // フィルタリング用（"ViewModel", "UserControl" など）

        public string Message { get; } // 実際のログ内容

        public LogEntry(string className, string identifier, string category, string message)
        {
            Timestamp = DateTime.Now;
            ClassName = className;
            Identifier = identifier;
            Category = category;
            Message = message;
        }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}][{Category}][{ClassName}][{Identifier}] {Message}";
        }
    }
}
