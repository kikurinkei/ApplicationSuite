using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.Runtime.Registries;
using ApplicationSuite.WindowModules.AppShared.Base;
using ApplicationSuite.WindowModules.AppShared.Utilities.Logging;

namespace ApplicationSuite.WindowModules.AppShared.Utilities.LogView
{
    /// <summary>
    /// LogViewViewModel
    /// ログ表示のカテゴリ・検索フィルタリングを担うViewModel。
    /// 表示責務を明示的に分離し、UI側の選択状態に応じたログ抽出を行う。
    /// 設計思想：
    /// - Enumによる抽象化は避け、カテゴリごとにプロパティを分離
    /// - 責務の明示と拡張性の担保（カテゴリ追加はプロパティ単位で対応可能）
    /// - ログ表示は文字列として整形し、UIに直接バインド可能な形式で保持
    /// </summary>
    public class LogViewViewModel : BaseViewModel, ISelectedAware
    {
        // カテゴリ選択プロパティ群（UIのチェック状態と連動）
        public bool IsViewModelOn { get => _isViewModelOn; set { SetProperty(ref _isViewModelOn, value); ApplyFilter(); } }
        public bool IsUserControlOn { get => _isUserControlOn; set { SetProperty(ref _isUserControlOn, value); ApplyFilter(); } }
        public bool IsDataTemplateOn { get => _isDataTemplateOn; set { SetProperty(ref _isDataTemplateOn, value); ApplyFilter(); } }
        public bool IsCompositeOn { get => _isCompositeOn; set { SetProperty(ref _isCompositeOn, value); ApplyFilter(); } }
        public bool IsNavigationOn { get => _isNavigationOn; set { SetProperty(ref _isNavigationOn, value); ApplyFilter(); } }
        public bool IsInjectorOn { get => _isInjectorOn; set { SetProperty(ref _isInjectorOn, value); ApplyFilter(); } }
        public bool IsWindowOn { get => _isWindowOn; set { SetProperty(ref _isWindowOn, value); ApplyFilter(); } }
        public bool IsOtherOn { get => _isOtherOn; set { SetProperty(ref _isOtherOn, value); ApplyFilter(); } }

        // カテゴリ選択状態の内部保持
        private bool _isViewModelOn, _isUserControlOn, _isDataTemplateOn, _isCompositeOn;
        private bool _isNavigationOn, _isInjectorOn, _isWindowOn, _isOtherOn;

        // テキスト検索プロパティ（部分一致でフィルタ）
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        // 表示対象ログ（フィルタ結果を文字列で保持）
        private string _logText = string.Empty;
        public string LogText
        {
            get => _logText;
            private set => SetProperty(ref _logText, value);
        }

        // 選択通知（ISelectedAwareインターフェース）
        public void OnSelected(string windowUniqueId, string elementId)
        {
            ApplyFilter();
        }

        // フィルタ処理（カテゴリ・検索テキストに応じてログ抽出）
        private void ApplyFilter()
        {
            var entries = LogRegistry.Instance.GetAll();

            // 選択されたカテゴリを収集
            var selectedCategories = new List<string>();
            if (IsViewModelOn) selectedCategories.Add("ViewModel");
            if (IsUserControlOn) selectedCategories.Add("UserControl");
            if (IsDataTemplateOn) selectedCategories.Add("DataTemplate");
            if (IsCompositeOn) selectedCategories.Add("Composite");
            if (IsNavigationOn) selectedCategories.Add("Navigation");
            if (IsInjectorOn) selectedCategories.Add("Injector");
            if (IsWindowOn) selectedCategories.Add("Window");
            if (IsOtherOn) selectedCategories.Add("その他");

            bool hasCategoryFilter = selectedCategories.Any();
            bool hasTextFilter = !string.IsNullOrWhiteSpace(SearchText);

            // 条件に応じてログを抽出
            var filtered = entries.Where(entry =>
                hasCategoryFilter && selectedCategories.Contains(entry.Category) ||
                hasTextFilter && (
                    entry.Message.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    entry.Identifier.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                !hasCategoryFilter && !hasTextFilter
            );

            // 表示用に整形
            LogText = string.Join(Environment.NewLine, filtered.Select(e => e.ToString()));
        }
    }
}