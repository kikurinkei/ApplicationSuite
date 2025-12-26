using ApplicationSuite.Runtime.Pairing;
using ApplicationSuite.Runtime.Registries;
using ApplicationSuite.Runtime.Windowing;
using ApplicationSuite.WindowModules.AppShared.Base;
using ApplicationSuite.WindowModules.AppShared.Utilities.Dashboard;
using ApplicationSuite.WindowModules.AppShared.Utilities.NavigationList;
using ApplicationSuite.WindowModules.SecondaryWindow.ManualView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.WindowModules.SecondaryWindow.ManualView.Services; // AnchorResolver 用


namespace ApplicationSuite.Runtime.Injection
{
    /// <summary>
    /// 各種ViewModelやCompositeViewModelに対して、初期設定値や構成情報を注入する静的クラス。
    /// CompositeViewModel, NavigationListViewModel, 各AVM要素に対して適切な初期化処理を集中管理する。
    /// また、WindowへのWindowUniqueIdの注入もここで統一的に処理する。
    /// </summary>
    public static class SecondaryWindowPropertyInjector
    {
        /// <summary>
        /// 指定されたウィンドウに対応するCompositeVM・ViewModel・Windowなどに対して
        /// WindowUniqueId や初期設定値、Composite参照などを注入する。
        /// </summary>
        /// <param name="windowUniqueId">対象ウィンドウの一意識別子</param>
        /// <param name="composite">対象のCompositeViewModelインスタンス</param>
        public static void Inject(
            string windowUniqueId,
            SecondaryCompositeViewModel composite,
            string? parentWindowId,
            string? parentSelectedElementId)

        {
            // チェック：ParentWindowId と ParentSelectedElementId は必須
            // AIさんの指示により、ここで必須チェックを追加。
            // SecondaryはOPEN用途として必須にしたいなら、ここでチェックしてreturn
            if (string.IsNullOrWhiteSpace(parentWindowId))
                throw new ArgumentException("parentWindowId is required for Secondary inject.", nameof(parentWindowId));

            if (string.IsNullOrWhiteSpace(parentSelectedElementId))
                throw new ArgumentException("parentSelectedElementId is required for Secondary inject.", nameof(parentSelectedElementId));




            // ★順序修正：最初に WindowUniqueId をセット
            composite.WindowUniqueId = windowUniqueId;

            // ★順序修正：その後に初期表示のVMをセット（これで OnSelected に正しい windowId が渡る）
            // --- CompositeVM の初期状態設定（画面切替の初期位置） ---
            composite.CurrentContentViewModel = composite.AVM["ManualView"];

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


            // --- PrimaryCompositeViewModel
            // --- SecondaryCompositeViewModel に親子ペア情報を登録 ---

            // 1-1. CompositeViewModelRegistry からCompositeViewModelを取得
            PrimaryCompositeViewModel PrimaryComposite =
                CompositeViewModelRegistry.Instance.Get(parentWindowId) as PrimaryCompositeViewModel;

            // 1-2. 親CompositeViewModelの PairedWindowUniqueId に 自分の WindowUniqueId をセット
            PrimaryComposite.PairedWindowUniqueId = windowUniqueId;


            // 2-1. CompositeViewModelRegistry から ManualViewViewModel を取得
            var ManualViewViewModel = ViewModelRegistry.Instance.Get(windowUniqueId, "ManualView") as ManualViewViewModel;

            // 2-2. 自分の CompositeViewModel の PairedWindowUniqueId に 親の WindowUniqueId をセット
            ManualViewViewModel.PairedWindowUniqueId = parentWindowId;

            // ここではCCVMを渡さない。（スクロールのフローを走らせないため。）
            //// 2-3. 親の CompositeViewModel の CCVM を 自分の ManualViewViewModel の AnchorId にセット
            //ManualViewViewModel.AnchorId = parentSelectedElementId;




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
                }
            }












            // --- SecondaryShell に対して WindowUniqueId を注入 ---
            var InstanceWindow = WindowRegistry.Instance.Get(windowUniqueId) as IShell;
            if (InstanceWindow != null)
            {
                InstanceWindow.WindowUniqueId = windowUniqueId;
                Console.WriteLine($"[Inject] Window.WindowUniqueId = {windowUniqueId}");
            }
        }
    }
}