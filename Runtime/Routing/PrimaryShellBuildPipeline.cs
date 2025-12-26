using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ApplicationSuite.AppGenerator.Catalog;
using ApplicationSuite.Runtime.Registries;
using ApplicationSuite.AppGenerator.Activation;
using ApplicationSuite.Runtime.Windowing;
using ApplicationSuite.WindowModules.AppShared.Base;
using ApplicationSuite.Runtime.Injection;

namespace ApplicationSuite.Runtime.Service.Routing
{
    public static class PrimaryShellBuildPipeline
    {
        public static void Process(
            string shellId,
            string windowUniqueId,
            string? parentWindowId,
            string? parentSelectedElementId
            )

        {
            /// Initialize Regist

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
            // 3) DataTemplate（flag=4）
            var templateDict = BitFilter.FilterByTypeFlag(baseDict, 4);
            TemplateBuilder.BuildAndRegister(templateDict);

            // 4) Composite（flag=8）
            var comDict = BitFilter.FilterByTypeFlag(baseDict, 8);
            //var comIds = comDict.Keys.ToList();
            PrimaryCompositeBuilder.BuildAndRegister(windowUniqueId, comDict);

            // 5) Navigation&DashboardList（flag=16）
            var navi_dashDict = BitFilter.FilterByTypeFlag(baseDict, 16);
            NavigationListBuilder.BuildAndRegister(windowUniqueId, navi_dashDict);

            // Dashboard の表示用エントリもここで登録
            DashboardBuilder.BuildAndRegister(windowUniqueId, navi_dashDict);

            // WindowApplicationSuite.WindowApplications.AppShared.Dashboard.DashboardEntriesBuilder.BuildAndRegister(windowUniqueId);

            // 7) Window 本体
            WindowBuilder.Build("Primary", windowUniqueId, shellId);

            /// Assemble

            // １. CompositeViewModelRegistry からCompositeViewModelを取得
            PrimaryCompositeViewModel composite =
                CompositeViewModelRegistry.Instance.Get(windowUniqueId) as PrimaryCompositeViewModel;

            // ２. CompositeViewModel、ViewModel に各種設定を注入
            PrimaryWindowPropertyInjector.Inject(windowUniqueId, composite, null,null);

            // ３. WindowRegistry からウィンドウのインスタンスを取得
            Window window = WindowRegistry.Instance.Get(windowUniqueId) as Window
                ?? throw new InvalidOperationException("Window インスタンスの取得に失敗しました。");

            // ４. Window に CompositeViewModel を接続
            window.DataContext = composite;


            // ウィンドウの表示
            window.Show();


        }

    }
}
