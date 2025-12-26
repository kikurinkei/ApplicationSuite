using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ApplicationSuite.WindowModules.AppShared.Base;

namespace ApplicationSuite.WindowModules.UtilityTools.LineReplaceRegex
{
    /*
     * UC Summary (LineReplaceRegex):
     * 目的: 各行に正規表現置換を適用する（Pattern → Replace）。行順保持。
     * 入力: InputText, FindText(Regex), ReplaceText, IgnoreCase(bool), UseMultiline(bool)
     * 操作: Replace実行 / Initialize（オールクリア）
     * 仕様: RegexOptions は IgnoreCase/Multiline のみ初稿対応。空の Pattern は無操作扱い。
     * 手順: Pattern/Replace を入力 → オプション選択 → Replace実行 → 出力確認（必要なら Initialize）
     */

    public class LineReplaceRegexViewModel : BaseViewModel
    {
        // 共通
        private string _inputText = string.Empty;
        public string InputText { get => _inputText; set => SetProperty(ref _inputText, value); }

        private string _outputText = string.Empty;
        public string OutputText { get => _outputText; set => SetProperty(ref _outputText, value); }

        // UC固有
        private string _findText = string.Empty;
        public string FindText { get => _findText; set => SetProperty(ref _findText, value ?? string.Empty); }

        private string _replaceText = string.Empty;
        public string ReplaceText { get => _replaceText; set => SetProperty(ref _replaceText, value ?? string.Empty); }

        private bool _ignoreCase = false;
        public bool IgnoreCase { get => _ignoreCase; set => SetProperty(ref _ignoreCase, value); }

        private bool _useMultiline = false;
        public bool UseMultiline { get => _useMultiline; set => SetProperty(ref _useMultiline, value); }

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

        private readonly LineReplaceRegexProcessor _processor = new();

        public LineReplaceRegexViewModel()
        {
            OperationItems.Add(new OperationItem("Replace実行", "Replace"));
            OperationItems.Add(new OperationItem("初期化", "Initialize"));
        }

        private void Dispatch(string operationName)
        {
            switch (operationName)
            {
                case "Replace":
                    OutputText = _processor.Run(operationName, InputText, FindText, ReplaceText, IgnoreCase, UseMultiline);
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
            IgnoreCase = false;
            UseMultiline = false;
        }
    }

    public sealed class OperationItem
    {
        public string DisplayText { get; }
        public string OperationName { get; }
        public OperationItem(string displayText, string operationName) { DisplayText = displayText; OperationName = operationName; }
    }
}
