using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ApplicationSuite.WindowModules.AppShared.Base;

namespace ApplicationSuite.WindowModules.UtilityTools.SpaceCompact
{
    /*
     * UC Summary (SpaceCompact)：
     * 目的：各行の「連続する空白類」を圧縮する。見た目の密度を下げ、正規化する。
     * 操作：
     *   - Spaces圧縮      : 半角スペース ' ' の連続を 1 個に圧縮（2 個以上 → 1）
     *   - Whitespace圧縮  : 「半角スペース/タブ/全角スペース」の連続を 1 個の半角スペースに正規化
     *   - Tabs圧縮        : タブ '\t' の連続を 1 個に圧縮
     *   - Initialize       : オールクリア
     * 仕様：
     *   - 改行は保持。順序保持。空行はそのまま。
     *   - Whitespace圧縮は混在ラン（例: " \t　 "）を **半角スペース1個** に統一。
     * 想定手順：操作を押す → 出力確認。必要なら Initialize。
     */

    public class SpaceCompactViewModel : BaseViewModel
    {
        // 共通プロパティ
        private string _inputText = string.Empty;
        public string InputText
        {
            get => _inputText;
            set => SetProperty(ref _inputText, value);
        }

        private string _outputText = string.Empty;
        public string OutputText
        {
            get => _outputText;
            set => SetProperty(ref _outputText, value);
        }

        // Operationボタン群（表示名/Op名のペア）
        public ObservableCollection<OperationItem> OperationItems { get; } = new();

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
                        _selectedOperation = null; // 連続押下許可
                        OnPropertyChanged(nameof(SelectedOperation));
                    }
                }
            }
        }

        private readonly SpaceCompactProcessor _processor = new();

        public SpaceCompactViewModel()
        {
            // 並び：Spaces圧縮 → Whitespace圧縮 → Tabs圧縮 → 初期化
            OperationItems.Add(new OperationItem("Spaces圧縮", "CollapseSpaces"));
            OperationItems.Add(new OperationItem("Whitespace圧縮", "CollapseMixedWhitespace"));
            OperationItems.Add(new OperationItem("Tabs圧縮", "CollapseTabs"));
            OperationItems.Add(new OperationItem("初期化", "Initialize"));
        }

        private void Dispatch(string operationName)
        {
            switch (operationName)
            {
                case "CollapseSpaces":
                case "CollapseMixedWhitespace":
                case "CollapseTabs":
                    OutputText = _processor.Run(operationName, InputText);
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
            OutputText = string.Empty;
        }
    }

    // 表示用アイテム
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
