using ApplicationSuite.AppGenerator.Catalog;
using ApplicationSuite.AppGenerator.Configuration;
using ApplicationSuite.Runtime.Service.Routing;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;


namespace ApplicationSuite
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);// エンコード
            Console.SetOut(new DebugTextWriter()); // イミディエイトウィンドウへの出力設定

            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // 構造系JSON
            ConfigReader.LoadAll();
            // 初期値系JSON
////////////// InitialValueReader.LoadAll();


            // デフォルトスイートIDを取得（null の場合は null）
            string? suiteId = ConfigResolver.GetSuiteRootId();

            // デフォルトオープンのシェルIDを取得（Suite ID が null の場合は null）
            string? shellId = suiteId != null ? ConfigResolver.GetShellEntryId(suiteId) : null;


            //　全ShellIDを取得し、shellIをキーにして、ChildElementIdsとそのElement値を辞書化する。
            // 先のソースはバックアップから復元して利用する。
            //ShellHierarchyRegistryBuilder.Build();

            

            base.OnStartup(e);


            // メインウインドウ起動（Shell ID 指定）
            //RunShell(shellId);

            Console.WriteLine($"[WindowLifecycleController] 未定義のステータス: ");


            PrimaryShellLifecycleController.HandleWindowLifecycle("OPEN", shellId, "", null, null);





            /////////////AppEntry.Start(); // AppEntry へのブリッジ

            ///////////////Console.WriteLine("[App.xaml.cs] アプリを終了します。");
        }

        /// <summary>
        /// 指定シェルIDを起点に INITIATE し、レジストリが空ならアプリを終了する
        /// </summary>
        /// <param name="shellId">起動するシェルID</param>
        //private bool RunShell(string shellId)
        //{
        //    WindowLifecycleController.windowLifecycleController(shellId, "INITIATE");


        //}

        public class DebugTextWriter : TextWriter // イミディエイトウィンドウへの出力設定
        {
            public override Encoding Encoding => Encoding.UTF8;
            public override void Write(string? value) => Debug.WriteLine(value);
            public override void WriteLine(string? value) => Debug.WriteLine(value);
        }
    }
}