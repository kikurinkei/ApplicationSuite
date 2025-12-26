using ApplicationSuite.WindowModules.AppShared.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.UtilityTools.TestStringOperation
{

    public class TestStringOperationViewModel : BaseViewModel, ISelectedAware
    {
        public ObservableCollection<OperationItem> OperationItems { get; set; } = new();

        private OperationItem? _selectedOperation;
        public OperationItem? SelectedOperation
        {
            get => _selectedOperation;
            set
            {
                if (_selectedOperation != value)
                {
                    _selectedOperation = value;
                    OnPropertyChanged(nameof(SelectedOperation));

                    if (value != null)
                    {
                        // 実行処理（ここでは Console 出力で代用）
                        Console.WriteLine($"実行: {value.OperationName} 対象: {value.TargetName}");
                    }
                }
            }
        }

        private string _someText = string.Empty;
        public string SomeText
        {
            get => _someText;
            set
            {
                if (_someText != value)
                {
                    _someText = value;
                    OnPropertyChanged(nameof(SomeText));
                }
            }
        }


        public void InitializeFromSetting(string windowUniqueId)
        {
            WindowUniqueId = windowUniqueId;
        }

        public void OnSelected(string windowUniqueId, string elementId)
        {
            WindowUniqueId = windowUniqueId;
            OperationItems.Clear();

            OperationItems.Add(new OperationItem { DisplayText = " - EXECUTE Alpha", OperationName = "EXECUTE", TargetName = "Alpha" });
            OperationItems.Add(new OperationItem { DisplayText = " - EXECUTE Beta", OperationName = "EXECUTE", TargetName = "Beta" });
        }

        public class OperationItem
        {
            public string DisplayText { get; set; } = string.Empty;
            public string OperationName { get; set; } = string.Empty;
            public string TargetName { get; set; } = string.Empty;
        }
    }
}
