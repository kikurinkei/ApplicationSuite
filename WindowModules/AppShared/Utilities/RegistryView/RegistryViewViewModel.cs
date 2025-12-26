using ApplicationSuite.Runtime.Registries;
using ApplicationSuite.Runtime.Windowing;
using ApplicationSuite.WindowModules.AppShared.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.AppShared.Utilities.RegistryView
{
    // ------------------------------------------------------------
    // レジストリ群から情報を集約し、テキストで表示するためのViewModel。
    // RegistryViewUserControl.xaml の TextBox にバインドされます。
    // ------------------------------------------------------------
    public class RegistryViewViewModel : BaseViewModel, ISelectedAware
    {

        public void OnSelected(string windowUniqueId, string elementId)
        {
            selectedWindowUniqueId = windowUniqueId;
            OnPropertyChanged(nameof(RegistrySummaryText));
        }

        // CompositeViewModelなどから注入される、現在選択中のwindowUniqueId
        private string selectedWindowUniqueId = string.Empty;

        // 外部から選択中のIDを渡すためのプロパティ
        public void SetSelectedWindowUniqueId(string windowUniqueId)
        {
            selectedWindowUniqueId = windowUniqueId;
        }

        // テキスト表示用のプロパティ（TextBoxにバインド）
        public string RegistrySummaryText => BuildRegistrySummary();

        // 一覧出力用のメイン処理
        private string BuildRegistrySummary()
        {
            var sb = new StringBuilder();

            // 各レジストリからwindowUniqueIdの一覧を取得し、重複を除く
            var allIds = new HashSet<string>();

            var idsFromViewModel = ViewModelRegistry.Instance.GetAllKeys();
            var idsFromUserControl = UserControlRegistry.Instance.GetAllKeys();
            var idsFromNavList = NavigationListRegistry.Instance.GetKeys();
            var idsFromComposite = CompositeViewModelRegistry.Instance.GetKeys();
            var idsFromWindow = WindowRegistry.Instance.GetKeys();

            // 各IDリストを統合して一意なIDセットを作成
            foreach (var id in idsFromViewModel)
                allIds.Add(id);
            foreach (var id in idsFromUserControl)
                allIds.Add(id);
            foreach (var id in idsFromNavList)
                allIds.Add(id);
            foreach (var id in idsFromComposite)
                allIds.Add(id);
            foreach (var id in idsFromWindow)
                allIds.Add(id);

            // 各IDごとに情報を出力
            foreach (var id in allIds)
            {
                // 現在のIDが選択中のIDと一致していれば "+ID" として出力
                if (id == selectedWindowUniqueId)
                {
                    sb.AppendLine("+" + id);
                }
                else
                {
                    sb.AppendLine(id);
                }

                sb.AppendLine(new string('-', 25));

                // --- ViewModel ---
                var viewModelKeys = ViewModelRegistry.Instance.GetKeys(id);
                List<string> viewModelTypes = new List<string>();

                foreach (var key in viewModelKeys)
                {
                    var vm = ViewModelRegistry.Instance.Get(id, key);
                    if (vm != null)
                    {
                        viewModelTypes.Add(vm.GetType().Name);
                    }
                }

                if (viewModelTypes.Count > 0)
                    sb.AppendLine("ViewModel: " + string.Join(", ", viewModelTypes));
                else
                    sb.AppendLine("ViewModel: NULL");

                // --- UserControl ---
                var userControlKeys = UserControlRegistry.Instance.GetKeys(id);
                List<string> ucTypes = new List<string>();

                foreach (var key in userControlKeys)
                {
                    var uc = UserControlRegistry.Instance.Get(id, key);
                    if (uc != null)
                    {
                        ucTypes.Add(uc.GetType().Name);
                    }
                }

                if (ucTypes.Count > 0)
                    sb.AppendLine("UserControl: " + string.Join(", ", ucTypes));
                else
                    sb.AppendLine("UserControl: NULL");

                // --- NavigationList ---
                var navItems = NavigationListRegistry.Instance.Get(id);
                if (navItems != null && navItems.Count > 0)
                {
                    List<string> elementIds = new List<string>();
                    foreach (var item in navItems)
                    {
                        elementIds.Add(item.ElementId); // ※DisplayNameは使用しない
                    }

                    sb.AppendLine("NavigationList: " + string.Join(", ", elementIds));
                }
                else
                {
                    sb.AppendLine("NavigationList: NULL");
                }

                // --- CompositeViewModel ---
                var composite = CompositeViewModelRegistry.Instance.Get(id);
                if (composite != null)
                {
                    sb.AppendLine("CompositeViewModel: " + composite.GetType().Name);
                }
                else
                {
                    sb.AppendLine("CompositeViewModel: NULL");
                }

                // --- Window ---
                var window = WindowRegistry.Instance.Get(id);
                sb.AppendLine("[Base]Window: " + (window != null ? "YES" : "NO"));

                // 空行
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}