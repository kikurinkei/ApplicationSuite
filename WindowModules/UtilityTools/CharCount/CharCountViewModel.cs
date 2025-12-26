using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ApplicationSuite.WindowModules.AppShared.Base;

namespace ApplicationSuite.WindowModules.UtilityTools.CharCount
{
    /*
     * UC Summary (CharCount)：
     * 目的：テキストの「バイト風長さ」を計算する（半角=1 / 全角=2）。行数などは計上しない。
     * 入力：InputText
     * 操作：Count / Initialize（オールクリア）
     * 仕様：全角・半角の判定は Unicode の代表的レンジに基づく簡易ヒューリスティック（和中韓=2、全角記号=2、Halfwidth Katakana/ASCII=1）。改行は 1 文字=1 としてカウント。
     */

    public class CharCountViewModel : BaseViewModel
    {
        private string _inputText = string.Empty;
        public string InputText { get => _inputText; set => SetProperty(ref _inputText, value); }

        private string _outputText = string.Empty;
        public string OutputText { get => _outputText; set => SetProperty(ref _outputText, value); }

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

        private readonly CharCountProcessor _processor = new();

        public CharCountViewModel()
        {
            OperationItems.Add(new OperationItem("Count", "Count"));
            OperationItems.Add(new OperationItem("初期化", "Initialize"));
        }

        private void Dispatch(string operationName)
        {
            switch (operationName)
            {
                case "Count":
                    // 出力は数値を文字列化（必要に応じて「Count: n」等に変更可）
                    OutputText = _processor.Run(operationName, InputText);
                    break;
                case "Initialize":
                    InputText = string.Empty; OutputText = string.Empty; break;
                default:
                    OutputText = InputText; break;
            }
        }
    }

    public sealed class OperationItem
    {
        public string DisplayText { get; }
        public string OperationName { get; }
        public OperationItem(string displayText, string operationName) { DisplayText = displayText; OperationName = operationName; }
    }
}

