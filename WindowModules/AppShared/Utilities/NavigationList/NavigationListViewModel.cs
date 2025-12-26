using ApplicationSuite.AppGenerator.Activation.Models;
using ApplicationSuite.WindowModules.AppShared.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.AppShared.Utilities.NavigationList
{
    // ======================================
    // NavigationListViewModel.cs
    // --------------------------------------
    // View とのバインディング、選択処理、AVM連携を行う
    // ======================================

    /// <summary>
    /// ナビゲーションリストのビューモデル
    /// </summary>

    public class NavigationListViewModel : BaseViewModel //, INavigationListViewModel
    {
        public List<NavigationListItem> Items { get; private set; } = new();

        private NavigationListItem? _selectedItem;
        public NavigationListItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    if (value != null && _composite != null)
                    {
                        if (_composite.AVM.ContainsKey(value.ElementId))
                        {
                            _composite.CurrentContentViewModel = _composite.AVM[value.ElementId];
                        }
                        else
                        {
                            // Handle the missing key (e.g., log, throw a custom exception, or set to null)
                        }
                    }
                }
            }
        }

        private ICompositeViewModel? _composite;
        public void SetCompositeViewModel(ICompositeViewModel composite)
        {
            _composite = composite;
        }

        public void SetItems(List<NavigationListItem> items)
        {
            Items = items;
            SelectedItem = items.FirstOrDefault();
        }
    }
}

