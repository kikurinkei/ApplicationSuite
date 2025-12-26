using ApplicationSuite.WindowModules.AppShared.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace ApplicationSuite.WindowModules.UtilityTools.LineSuffix
{
    /*
     * UC Summary (LineSuffix)：
     * 目的：各行の末尾に Suffix を〔追加／削除／切替〕する。Initialize はオールクリア。
     * 入力：InputText（複数行可）、Suffix（文字列・既定なし）
     * 操作：AddSuffix / RemoveSuffix / ToggleSuffix / Initialize
     * 仕様：末尾は完全一致で比較。空文字Suffixは無操作。行順序は保持。
     * 想定手順：Suffix入力 → 操作を押す → 出力確認。必要なら Initialize で全リセット。
     */

    public class LineSuffixViewModel : BaseViewModel
    {
        // ---- 共通プロパティ ----
        private string _inputText = string.Empty;
        public string InputText
        {
            get => _inputText;
            set => SetProperty(ref _inputText, value);
        }

        private string _suffix = string.Empty;
        public string Suffix
        {
            get => _suffix;
            set => SetProperty(ref _suffix, value ?? string.Empty);
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
        private readonly LineSuffixProcessor _processor = new();

        public LineSuffixViewModel()
        {
            // 並び：追加 → 削除 → 切替 → 初期化（予約）
            OperationItems.Add(new OperationItem("Suffix追加", "AddSuffix"));
            OperationItems.Add(new OperationItem("Suffix削除", "RemoveSuffix"));
            OperationItems.Add(new OperationItem("Suffix切替", "ToggleSuffix"));
            OperationItems.Add(new OperationItem("初期化", "Initialize"));
        }

        // ---- 浅い switch 一発 ----
        private void Dispatch(string operationName)
        {
            switch (operationName)
            {
                case "AddSuffix":
                case "RemoveSuffix":
                case "ToggleSuffix":
                    OutputText = _processor.Run(operationName, InputText, Suffix);
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
            Suffix = string.Empty;
            OutputText = string.Empty;
        }
    }

    // 見せ方だけ：DisplayText + OperationName（LinePrefix同型）
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
