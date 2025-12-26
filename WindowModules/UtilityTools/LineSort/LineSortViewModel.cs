using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ApplicationSuite.WindowModules.AppShared.Base;

namespace ApplicationSuite.WindowModules.UtilityTools.LineSort
{
    /*
     * UC Summary (LineSort)：
     * 目的：テキストを行単位でソートする（昇順/降順）。空行も対象。
     * 入力：InputText
     * 操作：昇順 / 降順 / Initialize（オールクリア）
     * 仕様：StringComparer.Ordinal による決定的な比較。改行は基盤で正規化し、結合は末尾改行なし:contentReference[oaicite:3]{index=3}。
     */

    public class LineSortViewModel : BaseViewModel
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

        private readonly LineSortProcessor _processor = new();

        public LineSortViewModel()
        {
            OperationItems.Add(new OperationItem("昇順", "SortAsc"));
            OperationItems.Add(new OperationItem("降順", "SortDesc"));
            OperationItems.Add(new OperationItem("初期化", "Initialize"));
        }

        private void Dispatch(string operationName)
        {
            switch (operationName)
            {
                case "SortAsc":
                    OutputText = _processor.Run(operationName, InputText, ascending: true);
                    break;
                case "SortDesc":
                    OutputText = _processor.Run(operationName, InputText, ascending: false);
                    break;
                case "Initialize":
                    InputText = string.Empty; OutputText = string.Empty;
                    break;
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
