/*
SUMMARY (FIX):
- BaseViewModel を継承するように変更。
- INotifyPropertyChanged/Notify の自前実装を削除。
- プロパティは SetProperty を利用して変更通知。
*/

using System.Collections.ObjectModel;
using ApplicationSuite.WindowModules.AppShared.Base; // BaseViewModel の namespace

namespace ApplicationSuite.WindowModules.UtilityTools.LinePrefix
{
    /*
 * UC Summary (LinePrefix)：
 * 目的：各行の先頭に Prefix を〔追加／削除／切替〕する。Initialize はオールクリア。
 * 入力：InputText（複数行可）、Prefix（文字列・既定なし）
 * 操作：AddPrefix / RemovePrefix / TogglePrefix / Initialize
 * 仕様：行頭が prefix と一致で比較。空文字Prefixは無操作。行順序は保持。
 * 想定手順：prefix入力 → 操作を押す → 出力確認。必要なら Initialize で全リセット。
 */
    public class LinePrefixViewModel : BaseViewModel
    {
        // ---- 最小プロパティ ----
        private string _inputText = string.Empty;
        public string InputText
        {
            get => _inputText;
            set => SetProperty(ref _inputText, value);
        }

        private string _prefix = string.Empty;
        public string Prefix
        {
            get => _prefix;
            set => SetProperty(ref _prefix, value ?? string.Empty);
        }

        private string _outputText = string.Empty;
        public string OutputText
        {
            get => _outputText;
            set => SetProperty(ref _outputText, value);
        }

        private OperationItem? _selectedOperation;
        public OperationItem? SelectedOperation
        {
            get => _selectedOperation;
            set
            {
                if (SetProperty(ref _selectedOperation, value))
                {
                    if (_selectedOperation != null)
                    {
                        Dispatch(_selectedOperation.OperationName);
                        // 連続押下を許可するためリセット
                        _selectedOperation = null;
                        OnPropertyChanged(nameof(SelectedOperation));
                    }
                }
            }
        }

        public ObservableCollection<OperationItem> OperationItems { get; } = new();

        // ---- Processor（窓口） ----
        private readonly LinePrefixProcessor _processor = new();

        public LinePrefixViewModel()
        {
            // 初稿: 値JSONは使わない。OperationItemを手で定義。
            OperationItems.Add(new OperationItem("Prefix追加", "AddPrefix"));
            OperationItems.Add(new OperationItem("Prefix削除", "RemovePrefix"));
            OperationItems.Add(new OperationItem("Prefix切替", "TogglePrefix"));
            OperationItems.Add(new OperationItem("全行クリア", "Initialize"));

        }

        // ---- 浅い switch 一発 ----
        private void Dispatch(string operationName)
        {
            switch (operationName)
            {
                case "AddPrefix":
                    OutputText = _processor.Run(operationName, InputText, Prefix);
                    break;
                case "RemovePrefix":
                    OutputText = _processor.Run(operationName, InputText, Prefix);
                    break;
                case "TogglePrefix":
                    OutputText = _processor.Run(operationName, InputText, Prefix);
                    break;
                case "Initialize":
                    ResetAll();
                    break;
                default:
                    OutputText = InputText;
                    break;
            }
        }
        private void ResetAll()
        {
            InputText = string.Empty;
            Prefix = string.Empty;
            OutputText = string.Empty;
        }
    }

    // ---- OperationItem（見せ方だけ：DisplayText + OperationName） ----
    public sealed class OperationItem
    {
        public string DisplayText { get; }
        public string OperationName { get; }
        public OperationItem(string displayText, string operationName)
        {
            DisplayText = displayText;
            OperationName = operationName;
        }
    }
}
