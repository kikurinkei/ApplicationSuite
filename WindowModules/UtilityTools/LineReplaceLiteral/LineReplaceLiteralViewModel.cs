using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ApplicationSuite.WindowModules.AppShared.Base;

namespace ApplicationSuite.WindowModules.UtilityTools.LineReplaceLiteral
{
    /*
     * UC Summary (LineReplaceLiteral):
     * 目的: 正規表現を使わず **文字列そのまま** の置換を行う（Find → Replace）。行順保持。
     * 入力: InputText, FindText, ReplaceText, CaseSensitive(bool)
     * 操作: Replace実行 / Initialize（オールクリア）
     * 仕様: CaseSensitive=false の場合も、文化非依存・Ordinal ベースで比較（Regexを内部利用）。
     * 手順: Find/Replace を入力 → 必要なら CaseSensitive → Replace実行 → 出力確認（必要なら Initialize）
     */

    public class LineReplaceLiteralViewModel : BaseViewModel
    {
        private string _inputText = string.Empty;
        public string InputText { get => _inputText; set => SetProperty(ref _inputText, value); }

        private string _outputText = string.Empty;
        public string OutputText { get => _outputText; set => SetProperty(ref _outputText, value); }

        private string _findText = string.Empty;
        public string FindText { get => _findText; set => SetProperty(ref _findText, value ?? string.Empty); }

        private string _replaceText = string.Empty;
        public string ReplaceText { get => _replaceText; set => SetProperty(ref _replaceText, value ?? string.Empty); }

        private bool _caseSensitive = true;
        public bool CaseSensitive { get => _caseSensitive; set => SetProperty(ref _caseSensitive, value); }

        public ObservableCollection<OperationItem> OperationItems { get; } = new();
        private OperationItem? _selectedOperation;
        public OperationItem? SelectedOperation
        {
            get => _selectedOperation;
            set
            {
                if (SetProperty(ref _selectedOperation, value) && _selectedOperation != null)
                {
                    Dispatch(_selectedOperation.OperationName);
                    _selectedOperation = null; OnPropertyChanged(nameof(SelectedOperation));
                }
            }
        }

        private readonly LineReplaceLiteralProcessor _processor = new();

        public LineReplaceLiteralViewModel()
        {
            OperationItems.Add(new OperationItem("Replace実行", "Replace"));
            OperationItems.Add(new OperationItem("初期化", "Initialize"));
        }

        private void Dispatch(string operationName)
        {
            switch (operationName)
            {
                case "Replace":
                    OutputText = _processor.Run(operationName, InputText, FindText, ReplaceText, CaseSensitive);
                    break;
                case "Initialize":
                    ResetAll(); break;
                default:
                    OutputText = InputText; break;
            }
        }

        private void ResetAll()
        {
            InputText = string.Empty;
            OutputText = string.Empty;
            FindText = string.Empty;
            ReplaceText = string.Empty;
            CaseSensitive = true;
        }
    }

    public sealed class OperationItem
    {
        public string DisplayText { get; }
        public string OperationName { get; }
        public OperationItem(string displayText, string operationName) { DisplayText = displayText; OperationName = operationName; }
    }
}
