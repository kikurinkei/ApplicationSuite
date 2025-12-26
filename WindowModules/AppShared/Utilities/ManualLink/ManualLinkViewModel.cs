using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ApplicationSuite.WindowModules.AppShared.Base;
using ApplicationSuite.Runtime.Registries;
using ApplicationSuite.Runtime.Service.Routing;


namespace ApplicationSuite.WindowModules.AppShared.Utilities.ManualLink
{
    /// <summary>
    /// ManualLink の ViewModel。
    /// 右上の "Manual" クリックで SecondaryShellLifecycleController に「投げるだけ」を実行する。
    /// - コードビハインド禁止ポリシーに従い、動作は ICommand に集約。
    /// - コンストラクタにロジックは置かない（生成時注入に備え、プロパティ中心）。
    /// </summary>
    public class ManualLinkViewModel : BaseViewModel
    {
        #region Commands

        /// <summary>
        /// Manual ウィンドウを開くコマンド。
        /// 実行時に SecondaryShellLifecycleController.HandleWindowLifecycle("OPEN","ManualView","") を呼び出す。
        /// windowUniqueId は OPEN 分岐後に Controller 側で生成するため、ここでは空文字を渡す。
        /// </summary>
        public ICommand OpenManualCommand => _openManualCommand;

        // 既定方針（デリゲート/ラムダ最小化）に合わせ、専用 ICommand 実装を用いる。
        private static readonly ICommand _openManualCommand = new ManualLinkOpenCommand();

        #endregion

        // --- 初期化口（外側が必要なら呼ぶ） ---
        public void InitializeFromSetting(string windowUniqueId)
        {

            WindowUniqueId = windowUniqueId;

            // 外部レジストリ等がここでコマンドを後差しする想定
            // ここでは何もしない（仕様追加なし）
        }



        #region Inner Command

        /// <summary>
        /// Manual を開くための専用コマンド実装（デリゲート非使用）。
        /// </summary>

        private sealed class ManualLinkOpenCommand : ICommand
        {
            public bool CanExecute(object parameter) => true;
            public event EventHandler CanExecuteChanged { add { } remove { } }

            public void Execute(object parameter)
            {
                // 1) 親(Primary)の WindowUniqueId を受け取る（XAML から渡ってくる）
                var parentWindowId = parameter as string ?? string.Empty;

                // 2) 親の CompositeVM をレジストリから取得
                var composite = CompositeViewModelRegistry.Instance.Get(parentWindowId);
                string elementId = string.Empty;

                if (composite != null)
                {
                    // 3) 現在表示中の VM を取得
                    var current = composite.CurrentContentViewModel;

                    if (current != null && composite.AVM != null)
                    {
                        // 4) AVM を逆引きして elementId を求める（最初に一致したキー）
                        foreach (var kv in composite.AVM)
                        {
                            if (object.ReferenceEquals(kv.Value, current))
                            {
                                elementId = kv.Key;
                                break;
                            }
                        }
                    }
                }

                // 5) Secondary へ OPEN を投げる（elementId を添付）
                SecondaryShellLifecycleController.HandleWindowLifecycle(
                    status: "OPEN",
                    shellId: "SecondaryWindow",
                    windowUniqueId: "",               // OPENは未指定でOK（Controller側で生成）
                    parentWindowId: parentWindowId,   // 親（Primary）のWindowUniqueId
                    parentSelectedElementId: elementId
                );

            }
        }
        #endregion
    }

}