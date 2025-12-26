using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ApplicationSuite.WindowModules.AppShared.Base;

namespace ApplicationSuite.WindowModules.UtilityTools.SpaceConvert
{
    /*
     * UC Summary (SpaceConvert)：
     * 目的：各行に含まれる空白を「Tabs↔Spaces」「全角↔半角（スペース）」で相互変換する。
     * 入力：InputText（複数行可）、TabWidth（整数、既定=4）
     * 操作：
     *   - Tabs→Spaces（TabWidth 個のスペースに置換：単純置換モデル）
     *   - Spaces→Tabs（連続 TabWidth 個の半角スペースを \t に圧縮：繰り返し）
     *   - 全角→半角（U+3000 → ' '）
     *   - 半角→全角（' ' → U+3000）
     *   - Initialize（オールクリア）
     * 仕様：
     *   - Tabs→Spaces は**位置に基づくタブストップ展開ではなく**「1タブ=TabWidthスペース」の単純置換。
     *   - Spaces→Tabs は「ちょうど TabWidth 連続の半角スペース」を \t に置換（複数回適用で連続領域も圧縮）。
     *   - 改行は保持。順序保持。空行はそのまま。
     * 想定手順：Tab幅を確認 → 操作を押す → 出力確認。必要なら Initialize。
     */

    public class SpaceConvertViewModel : BaseViewModel
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

        // UC固有：Tab幅（Tabs↔Spaces のみ使用）
        private int _tabWidth = 4; // 既定値
        public int TabWidth
        {
            get => _tabWidth;
            set => SetProperty(ref _tabWidth, value < 1 ? 1 : value); // 1未満は1に丸め
        }

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

        private readonly SpaceConvertProcessor _processor = new();

        public SpaceConvertViewModel()
        {
            // 並び：Tabs→Spaces → Spaces→Tabs → 全角→半角 → 半角→全角 → 初期化
            OperationItems.Add(new OperationItem("Tabs→Spaces", "TabsToSpaces"));
            OperationItems.Add(new OperationItem("Spaces→Tabs", "SpacesToTabs"));
            OperationItems.Add(new OperationItem("全角→半角", "ZenkakuToHankaku"));
            OperationItems.Add(new OperationItem("半角→全角", "HankakuToZenkaku"));
            OperationItems.Add(new OperationItem("初期化", "Initialize"));
        }

        private void Dispatch(string operationName)
        {
            switch (operationName)
            {
                case "TabsToSpaces":
                    OutputText = _processor.Run(operationName, InputText, TabWidth);
                    break;

                case "SpacesToTabs":
                    OutputText = _processor.Run(operationName, InputText, TabWidth);
                    break;

                case "ZenkakuToHankaku":
                case "HankakuToZenkaku":
                    OutputText = _processor.Run(operationName, InputText, tabWidth: null);
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
            TabWidth = 4; // 既定に戻す
        }
    }

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
