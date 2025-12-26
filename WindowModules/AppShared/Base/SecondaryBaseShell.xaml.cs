using ApplicationSuite.Runtime.Registries;
using ApplicationSuite.Runtime.Service.Routing;
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

namespace ApplicationSuite.WindowModules.AppShared.Base
{
    /// <summary>
    /// セカンダリ（Manual/Log 用の器）。マーカー段階ではイベントフックなし。
    /// </summary>
    public partial class SecondaryBaseShell : System.Windows.Window, IShell
    {
        private string? _windowUniqueId;
        public string? WindowUniqueId { get => _windowUniqueId; set => _windowUniqueId = value; }

        /// <summary>
        /// ウィンドウが閉じる処理が進行中かどうかを示すフラグ。
        /// trueの場合、ウィンドウが閉じる処理が進行中であることを示します。
        /// </summary>
        public bool? IsClosingInProgress { get; set; } = false;
        public bool? IsRestartRequested { get; set; } = false;


        public new void Show() => base.Show();
        public new void Close() => base.Close();

        public SecondaryBaseShell()
        {
            InitializeComponent();

            this.Closing += OnClosing; // これを使うと、ウィンドウが閉じる前に特定の処理を実行できます。
            this.Closed += OnClosed; // これを使うと、ウィンドウが閉じた後に特定の処理を実行できます。
        }
        private void OnClosing(object? sender, CancelEventArgs e)
        {
            if (IsClosingInProgress == false)
            {
                // フラグを設定して、閉じる処理が進行中であることを示す
                IsClosingInProgress = true;
                //　windowLifecycleControllerのウィンドウを閉じる処理を呼び出す
                SecondaryShellLifecycleController.HandleWindowLifecycle("CLOSE", "", WindowUniqueId ?? "", null, null);


            }
            else if (IsClosingInProgress == true)
            {
                // すでに閉じる処理が進行中の場合は何もしない
            }
        }
        private void OnClosed(object? sender, EventArgs e)
        {
            // ウィンドウが閉じた後の処理をここに記述できます。
            // 例えば、リソースの解放やログの記録など。

            RegistryCleaner.ClearOne(WindowUniqueId);

            if (IsRestartRequested == false && RegistryCleaner.AreAllEmpty())
            {
                Application.Current.Shutdown();
            }
            else
            {
                // RESTART の場合は何もしない
            }
        }
    }
}