using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.AppShared.Base
{

    /// <summary>
    /// 複数の ViewModel を管理・注入できる合成 ViewModel。
    /// 現在表示中の ViewModel（CurrentContentViewModel）とサイドナビゲーションも管理。
    /// </summary>
    public class SecondaryCompositeViewModel : ICompositeViewModel, INotifyPropertyChanged
    {
        public string? PairedWindowUniqueId { get; set; } // null許容型にした。

        public string? WindowUniqueId { get; set; } = null; // null許容型にした。

        public bool? IsRealtimeSyncEnabled { get; set; } // null許容型にした。

        public string? CCVM { get; set; } // null許容型にした。

        // elementId → ViewModel の辞書（AVM）
        private readonly Dictionary<string, BaseViewModel> _viewModels = new();

        private BaseViewModel? _currentContentViewModel;
        private BaseViewModel? _sideContentViewModel;

        /// <summary>
        /// 現在表示されている メインコンテンツの ViewModel。
        /// 切り替え時に ISelectedAware に通知を送る（ログ取得など）。
        /// </summary>
        public BaseViewModel? CurrentContentViewModel
        {
            get => _currentContentViewModel;
            set
            {
                if (_currentContentViewModel != value)
                {
                    _currentContentViewModel = value;
                    OnPropertyChanged(nameof(CurrentContentViewModel));

                    // ログ通知処理（ISelectedAware 実装に限る）
                    if (value is ISelectedAware aware)
                    {
                        // elementId を逆引き（AVMから探す）
                        var elementId = _viewModels.FirstOrDefault(kvp => kvp.Value == value).Key;
                        //var windowId = value.WindowUniqueId ?? "UnknownWindow";
                        var windowId = WindowUniqueId ?? "UnknownWindow";

                        if (!string.IsNullOrEmpty(elementId))
                        {
                            aware.OnSelected(windowId, elementId);

                        }
                    }
                }
            }
        }

        /// <summary>
        /// 現在表示されている サイドナビゲーションの ViewModel。
        /// </summary>
        public BaseViewModel? SideContentViewModel
        {
            get => _sideContentViewModel;
            set
            {
                if (_sideContentViewModel != value)
                {
                    _sideContentViewModel = value;
                    OnPropertyChanged(nameof(SideContentViewModel));
                }
            }
        }

        /// <summary>
        /// ViewModel を elementId に紐づけて登録・上書き。
        /// </summary>
        public void Inject(string elementId, BaseViewModel viewModel)
        {
            _viewModels[elementId] = viewModel;
        }

        /// <summary>
        /// ID で ViewModel を検索して取得。見つからない場合は null。
        /// </summary>
        public BaseViewModel? Resolve(string elementId)
        {
            return _viewModels.TryGetValue(elementId, out var vm) ? vm : null;
        }

        /// <summary>
        /// 登録済みの ViewModel 一覧（読み取り専用）。
        /// </summary>
        public IReadOnlyDictionary<string, BaseViewModel> AVM => _viewModels;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}