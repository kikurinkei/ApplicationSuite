using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ApplicationSuite.WindowModules.AppShared.Base;

namespace ApplicationSuite.WindowModules.UtilityTools.LineNumbering
{
    /*
     * UC Summary (LineNumbering)：
     * 目的：各行の先頭に「連番＋接続子」を付与/除去する。接続子は自由入力（例: ".", " ", " - "）。
     * 入力：InputText、StartNumber(int, 既定=1)、PadWidth(int, 既定=0)、Connector(string, 既定=".")
     * 操作：番号付与 / 番号削除 / Initialize（オールクリア）
     * 仕様：
     *   - 付与：行インデックス依存のため **順次処理**（非並列）。例：Start=1, PadWidth=2, Connector=". " → "01. 行"
     *   - 削除：行頭の「数字+Connector」があれば1回だけ除去（数字桁数は任意）。Connector は完全一致で判定。
     *   - 空行も番号付与対象（初稿仕様）。改行は保持、行順保持。
     * 手順：Start/Pad/Connector を設定 → 操作 → 出力確認。必要なら Initialize。
     */

    public class LineNumberingViewModel : BaseViewModel
    {
        // 共通
        private string _inputText = string.Empty;
        public string InputText { get => _inputText; set => SetProperty(ref _inputText, value); }

        private string _outputText = string.Empty;
        public string OutputText { get => _outputText; set => SetProperty(ref _outputText, value); }

        // UC固有：開始番号 / 桁幅 / 接続子
        private int _startNumber = 1;
        public int StartNumber { get => _startNumber; set => SetProperty(ref _startNumber, value < 0 ? 0 : value); }

        private int _padWidth = 0; // 0 = 桁埋めなし
        public int PadWidth { get => _padWidth; set => SetProperty(ref _padWidth, value < 0 ? 0 : value); }

        private string _connector = ".";
        public string Connector { get => _connector; set => SetProperty(ref _connector, value ?? "."); }

        // 操作
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

        private readonly LineNumberingProcessor _processor = new();

        public LineNumberingViewModel()
        {
            // 並び：付与 → 削除 → 初期化（統一パターン）
            OperationItems.Add(new OperationItem("番号付与", "AddNumbers"));
            OperationItems.Add(new OperationItem("番号削除", "RemoveNumbers"));
            OperationItems.Add(new OperationItem("初期化", "Initialize"));
        }

        private void Dispatch(string operationName)
        {
            switch (operationName)
            {
                case "AddNumbers":
                    OutputText = _processor.Run(operationName, InputText, StartNumber, PadWidth, Connector);
                    break;
                case "RemoveNumbers":
                    OutputText = _processor.Run(operationName, InputText, StartNumber, PadWidth, Connector);
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
            StartNumber = 1;
            PadWidth = 0;
            Connector = ".";
        }
    }

    public sealed class OperationItem
    {
        public string DisplayText { get; }
        public string OperationName { get; }
        public OperationItem(string displayText, string operationName) { DisplayText = displayText; OperationName = operationName; }
    }
}
