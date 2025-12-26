using System;
using System.Windows;
using ApplicationSuite.AppGenerator.Activation;
using ApplicationSuite.AppGenerator.Catalog;
using ApplicationSuite.Runtime.Injection;
using ApplicationSuite.Runtime.Registries;
using ApplicationSuite.Runtime.Windowing;
using ApplicationSuite.WindowModules.AppShared.Base;

namespace ApplicationSuite.Runtime.Service.Routing
{
    /// <summary>Secondary Shell の組み立て（親子リンク登録＋保留配達を追加）。</summary>
    public class SecondaryShellBuildPipeline
    {
        public static void Process(
            string shellId,
            string windowUniqueId,
            string? parentWindowId,
            string? parentSelectedElementId
            )
        {
            /// Initialize Regist

            //// -1) PairingHub（GLOBAL）を 1回だけ用意（既にあれば何もしない）
            ////     ※PrimaryCompositeViewModel を作る前に呼ぶ
            //PairingHubBuilder.EnsureBuilt();



            // 0) ShellBitDictionary を構築（Shell.ChildElementIds 順で挿入）
            var baseDict = BitDictionaryBuilder.BitDictionary(shellId);

            // SSOT 登録
            ShellBitDictionaryRegistry.Register(windowUniqueId, baseDict);

            // 1) ViewModel（flag=1）
            var vmDict = BitFilter.FilterByTypeFlag(baseDict, 1);
            ViewModelBuilder.BuildAndRegister(windowUniqueId, vmDict);

            // 2) UserControl（flag=2）
            var ucDict = BitFilter.FilterByTypeFlag(baseDict, 2);
            UserControlBuilder.BuildAndRegister(windowUniqueId, ucDict);

            // Template
            var templateDict = BitFilter.FilterByTypeFlag(baseDict, 4);
            TemplateBuilder.BuildAndRegister(templateDict);

            // 4) Composite（flag=8）
            var comDict = BitFilter.FilterByTypeFlag(baseDict, 8);
            SecondaryCompositeBuilder.BuildAndRegister(windowUniqueId, comDict);

            // 5) Navigation&DashboardList（flag=16）
            var navi_dashDict = BitFilter.FilterByTypeFlag(baseDict, 16);
            NavigationListBuilder.BuildAndRegister(windowUniqueId, navi_dashDict);

            // Dashboard の表示用エントリもここで登録
            DashboardBuilder.BuildAndRegister(windowUniqueId, navi_dashDict);

            // 7) Window 本体
            WindowBuilder.Build("Secondary", windowUniqueId, shellId);

            //if (!string.IsNullOrEmpty(parentWindowId))
            //{
            //    WindowRegistry.Instance.LinkParentChild(windowUniqueId, parentWindowId);
            //}
            /// Assemble

            // １. CompositeViewModelRegistry からCompositeViewModelを取得
            SecondaryCompositeViewModel composite
                = CompositeViewModelRegistry.Instance.Get(windowUniqueId) as SecondaryCompositeViewModel;

            // ２. CompositeViewModel、ViewModel に各種設定を注入
            SecondaryWindowPropertyInjector.Inject(
                windowUniqueId, composite, parentWindowId, parentSelectedElementId);

            // ３. WindowRegistry からウィンドウのインスタンスを取得
            Window window = WindowRegistry.Instance.Get(windowUniqueId) as Window
                ?? throw new InvalidOperationException("Window インスタンスの取得に失敗しました。");

            // ４. Window に CompositeViewModel を接続
            window.DataContext = composite;

            // ウィンドウの表示
            window.Show();

            // SecondaryShellLifecycleController.Bridge.DeliverBuffered(windowUniqueId);
        }
    }
}
