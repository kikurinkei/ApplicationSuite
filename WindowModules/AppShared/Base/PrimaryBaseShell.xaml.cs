
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ApplicationSuite.Runtime.Windowing.Close;
using ApplicationSuite.Runtime.Registries;
using ApplicationSuite.Runtime.Service.Routing;

namespace ApplicationSuite.WindowModules.AppShared.Base
{
    public partial class PrimaryBaseShell : System.Windows.Window, IShell
    {
        // --- IShell インターフェースの実装 ---

        // string? なので、nullを許容する形で実装
        public string? WindowUniqueId { get; set; }

        // bool? (Nullable<bool>) なので、型を合わせて実装
        public bool? IsClosingInProgress { get; set; } = false;
        public bool? IsRestartRequested { get; set; } = false;

        // Windowクラスが既に持っている Show/Close と衝突しないよう、
        // 明示的に実装するか、そのまま Window のメソッドを使用します
        void IShell.Show() => base.Show();
        void IShell.Close() => base.Close();

        // ------------------------------------



        // コンストラクタ等で Closing イベントを購読
        public PrimaryBaseShell()
        {
            InitializeComponent();
            this.Closing += OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (this.IsClosingInProgress == true) return;
            this.IsClosingInProgress = true;

            // 1. 管制塔経由でお掃除実行
            bool isReady = PrimaryShellLifecycleController.HandleWindowLifecycle(
                "CLOSE", "", this.WindowUniqueId ?? string.Empty, null, null);

            if (isReady)
            {
                // 2. 元のコードの精神を継承：
                // 再起動要求がなく、かつレジストリが全て空（最後の一人）なら、消灯（Shutdown）
                // ※RegistryCleaner.AreAllEmpty() が static で呼べる前提です
                if (this.IsRestartRequested != true && RegistryCleaner.AreAllEmpty())
                {
                    Application.Current.Shutdown();
                }

                // 最後の一人でなければ、このままメソッドを抜けることで、
                // このウィンドウだけが自然に閉じます（e.Cancel は false のまま）。
            }
            else
            {
                // お掃除失敗時は閉じさせない
                e.Cancel = true;
                this.IsClosingInProgress = false;
            }
        }
    }
}






//using ApplicationSuite.Runtime.Registries;
//using ApplicationSuite.Runtime.Service.Routing;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Shapes;

//namespace ApplicationSuite.WindowModules.AppShared.Base
//{
//    public partial class PrimaryBaseShell : System.Windows.Window, IShell
//    {
//        private string? _windowUniqueId;
//        public string? WindowUniqueId { get => _windowUniqueId; set => _windowUniqueId = value; }

//        /// <summary>
//        /// ウィンドウが閉じる処理が進行中かどうかを示すフラグ。
//        /// trueの場合、ウィンドウが閉じる処理が進行中であることを示します。
//        /// </summary>
//        public bool? IsClosingInProgress { get; set; } = false;
//        public bool? IsRestartRequested { get; set; } = false;

//        public new void Show() => base.Show();
//        public new void Close() => base.Close();

//        public PrimaryBaseShell()
//        {
//            InitializeComponent();

//            this.Closing += OnClosing; // これを使うと、ウィンドウが閉じる前に特定の処理を実行できます。
//            this.Closed += OnClosed; // これを使うと、ウィンドウが閉じた後に特定の処理を実行できます。
//        }
//        private void OnClosing(object? sender, CancelEventArgs e)
//        {
//            if (IsClosingInProgress == false)
//            {
//                // フラグを設定して、閉じる処理が進行中であることを示す
//                IsClosingInProgress = true;
//                //　windowLifecycleControllerのウィンドウを閉じる処理を呼び出す
//                PrimaryShellLifecycleController.HandleWindowLifecycle("CLOSE", "", WindowUniqueId, null, null);
//            }
//            else if (IsClosingInProgress == true)
//            {
//                // すでに閉じる処理が進行中の場合は何もしない
//            }
//        }
//        private void OnClosed(object? sender, EventArgs e)
//        {
//            // ウィンドウが閉じた後の処理をここに記述できます。
//            // 例えば、リソースの解放やログの記録など。

//            RegistryCleaner.ClearOne(WindowUniqueId);

//            if (IsRestartRequested == false && RegistryCleaner.AreAllEmpty())
//            {
//                Application.Current.Shutdown();
//            }
//            else
//            {
//                // RESTART の場合は何もしない
//            }
//        }
//    }
//}



