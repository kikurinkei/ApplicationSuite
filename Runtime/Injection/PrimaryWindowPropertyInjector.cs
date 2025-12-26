using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using ApplicationSuite.Runtime.Registries;
using ApplicationSuite.Runtime.Windowing;
using ApplicationSuite.WindowModules.AppShared.Base;
using ApplicationSuite.WindowModules.AppShared.Utilities.NavigationList;
using ApplicationSuite.WindowModules.AppShared.Utilities.Dashboard;

namespace ApplicationSuite.Runtime.Injection
{
    /// <summary>
    /// 各種ViewModelやCompositeViewModelに対して、初期設定値や構成情報を注入する静的クラス。
    /// CompositeViewModel, NavigationListViewModel, 各AVM要素に対して適切な初期化処理を集中管理する。
    /// また、WindowへのWindowUniqueIdの注入もここで統一的に処理する。
    /// </summary>
    public static class PrimaryWindowPropertyInjector
    {
        /// <summary>
        /// 指定されたウィンドウに対応するCompositeVM・ViewModel・Windowなどに対して
        /// WindowUniqueId や初期設定値、Composite参照などを注入する。
        /// </summary>
        /// <param name="windowUniqueId">対象ウィンドウの一意識別子</param>
        /// <param name="composite">対象のCompositeViewModelインスタンス</param>
        public static void Inject(
            string windowUniqueId,
            PrimaryCompositeViewModel composite,
            string? parentWindowId,
            string? parentSelectedElementId)

        {

            // ★順序修正：最初に WindowUniqueId をセット
            composite.WindowUniqueId = windowUniqueId;


            // ★順序修正：その後に初期表示のVMをセット（これで OnSelected に正しい windowId が渡る）
            // --- CompositeVM の初期状態設定（画面切替の初期位置） ---


            composite.CurrentContentViewModel = composite.AVM["Dashboard"];
            composite.SideContentViewModel = composite.AVM["NavigationList"];

            // --- NavigationListViewModel に Composite を注入し、ナビゲーション定義をセット ---
            var vmm = ViewModelRegistry.Instance.Get(windowUniqueId, "NavigationList") as NavigationListViewModel;
            var navItems = NavigationListRegistry.Instance.Get(windowUniqueId);

            if (vmm != null)
            {
                vmm.SetItems(navItems);
                vmm.SetCompositeViewModel(composite);
            }

            // --- NavigationListViewModel が AVM に含まれていれば、Compositeを明示注入 ---
            if (composite.AVM.TryGetValue("NavigationList", out var navBase)
                && navBase is NavigationListViewModel navListVM)
            {
                navListVM.SetCompositeViewModel(composite);

            }

            // ★追加: --- DashboardViewModel にも Composite を注入する（NavigationList と同等） ---
            // 1) Registry 経由で確実に注入
            var dvm = ViewModelRegistry.Instance.Get(windowUniqueId, "Dashboard") as DashboardViewModel;
            if (dvm != null)
            {
                dvm.SetCompositeViewModel(composite);
            }

            // 2) 念のため AVM に登録済みなら AVM 経由でも注入
            if (composite.AVM.TryGetValue("Dashboard", out var dashBase)
                && dashBase is DashboardViewModel dashVM)
            {
                dashVM.SetCompositeViewModel(composite);
            }


            // --- 各 ViewModel に対して WindowUniqueId を引数に InitializeFromSetting() を呼び出す ---
            foreach (var item in composite.AVM)
            {
                if (item.Value is BaseViewModel baseViewModel)
                {
                    Console.WriteLine($"[Injecting] ViewModel: {item.Key}");

                    // InitializeFromSetting(string windowUniqueId) があれば呼び出す
                    var method = item.Value?.GetType().GetMethod("InitializeFromSetting");

                    if (method != null)
                    {
                        method.Invoke(item.Value, new object[] { windowUniqueId });
                        Console.WriteLine($"[Success] {item.Key}: InitializeFromSetting invoked.");
                    }
                    else
                    {
                        Console.WriteLine($"[Skip] {item.Key}: No InitializeFromSetting method.");
                    }

                    // 必要であれば明示的に baseViewModel.WindowUniqueId を設定することも可能
                    // baseViewModel.WindowUniqueId = windowUniqueId;
                }
            }
            // --- Primary/SecondaryShell に対して WindowUniqueId を注入 ---
            var InstanceWindow = WindowRegistry.Instance.Get(windowUniqueId) as IShell;
            if (InstanceWindow != null)
            {
                InstanceWindow.WindowUniqueId = windowUniqueId;
                Console.WriteLine($"[Inject] Window.WindowUniqueId = {windowUniqueId}");
            }
        }
    }
}
