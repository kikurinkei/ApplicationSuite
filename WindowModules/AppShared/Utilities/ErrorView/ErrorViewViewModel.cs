using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.WindowModules.AppShared.Base;
using ApplicationSuite.WindowModules.AppShared.Utilities.Logging;

namespace ApplicationSuite.WindowModules.AppShared.Utilities.ErrorView
{
    /// <summary>
    /// ErrorViewViewModel
    /// エラー（Identifier = "Error"）専用のログ表示ViewModel。
    /// 表示責務を明示的に分離し、整形されたログをUIに提供する。
    /// 設計思想：
    /// - フィルタ条件を固定化し、責務を限定
    /// - 表示形式を整形し、UI側での加工不要に
    /// </summary>
    public class ErrorViewViewModel : BaseViewModel, ISelectedAware
    {
        // 表示対象ログ（整形済み）
        private string _logText = string.Empty;
        public string LogText
        {
            get => _logText;
            private set => SetProperty(ref _logText, value);
        }

        // 選択通知（ISelectedAwareインターフェース）
        public void OnSelected(string windowUniqueId, string elementId)
        {
            ApplyFilter();
        }

        // "Error"ログのみ抽出し、整形して表示
        private void ApplyFilter()
        {
            var entries = LogRegistry.Instance.GetAll();
            var filtered = entries.Where(e => e.Identifier == "Error");

            var builder = new StringBuilder();
            foreach (var entry in filtered)
            {
                builder.AppendLine($"[Timestamp] {entry.Timestamp:yyyy-MM-dd HH:mm:ss}");
                builder.AppendLine($"[ClassName] {entry.ClassName}");
                builder.AppendLine($"[Category]  {entry.Category}");
                builder.AppendLine($"[Identifier] {entry.Identifier}");
                builder.AppendLine($"[Message]   {entry.Message}");
                builder.AppendLine(); // 空行で区切る
            }

            LogText = builder.ToString();
        }
    }
}