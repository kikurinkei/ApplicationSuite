using System.Collections.ObjectModel;
using ApplicationSuite.WindowModules.AppShared.Base;

namespace ApplicationSuite.WindowModules.UtilityTools.LineTrim
{
    /*
     * UC Summary (LineTrim)：
     * 目的：各行の前後空白を削除する。TrimModeにより「左右 / 左のみ / 右のみ」を切替可能。
     * 入力：InputText（複数行可）、SelectedTrimMode（"Both"/"Left"/"Right"）
     * 操作：Trim実行 / Initialize
     * 仕様：改行は保持。空行はそのまま。順序保持。
     * 想定手順：Trimモード選択 → 操作を押す → 出力確認。必要なら Initialize。
     */

    public class LineTrimViewModel : BaseViewModel
    {
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

        // Trimモードの選択（左右/左/右）
        public ObservableCollection<string> TrimModes { get; } =
            new ObservableCollection<string>(new[] { "Both", "Left", "Right" });

        private string _selectedTrimMode = "Both"; // 既定値は左右
        public string SelectedTrimMode
        {
            get => _selectedTrimMode;
            set => SetProperty(ref _selectedTrimMode, value);
        }

        // Operationボタン群
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
                        _selectedOperation = null;
                        OnPropertyChanged(nameof(SelectedOperation));
                    }
                }
            }
        }

        private readonly LineTrimProcessor _processor = new();

        public LineTrimViewModel()
        {
            OperationItems.Add(new OperationItem("Trim実行", "Trim"));
            OperationItems.Add(new OperationItem("初期化", "Initialize"));
        }

        private void Dispatch(string operationName)
        {
            switch (operationName)
            {
                case "Trim":
                    // Processor.Run に TrimMode を渡す
                    OutputText = _processor.Run(InputText, SelectedTrimMode);
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
            SelectedTrimMode = "Both";
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
