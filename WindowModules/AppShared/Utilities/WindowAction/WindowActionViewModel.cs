using ApplicationSuite.Runtime.Service.Routing;
using ApplicationSuite.WindowModules.AppShared.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.AppShared.Utilities.WindowAction
{
    /// <summary>
    /// Window操作を行うためのコマンドViewModel。
    /// ユーザー操作により指定されたコマンドを受け取り、WindowCommandDispatcherに委譲する。
    /// </summary>
    /// 
    public class WindowActionViewModel : BaseViewModel, ISelectedAware
    {
        /// <summary>
        /// UIで表示されるコマンド一覧。ListBoxにバインドされる。
        /// </summary>
        public ObservableCollection<CommandItem> CommandItems { get; set; } = new();

        private CommandItem? _selectedCommand;
        /// <summary>
        /// ユーザーが選択したコマンド。
        /// 選択時に Dispatcher を呼び出して制御を委譲する。
        /// </summary>
        public CommandItem? SelectedCommand
        {
            get => _selectedCommand;
            set
            {
                if (_selectedCommand != value)
                {
                    _selectedCommand = value;
                    OnPropertyChanged(nameof(SelectedCommand));

                    if (value != null)
                    {
                        // コマンドが選択されたとき、Dispatcher に制御を渡す
                        // 重要：shellId（種別）と windowUniqueId（個体）を混ぜない
                        var command = (value.FirstCommand ?? "").Trim().ToUpperInvariant();
                        var targetName = (value.FirstTargetName ?? "").Trim();
                        var requestorWindowId = this.WindowUniqueId ?? "UNKNOWN";

                        switch (command)
                        {
                            case "OPEN":
                                // OPEN：shellId = 開きたい種別ID、windowUniqueId = ""（未指定）
                                if (string.IsNullOrWhiteSpace(targetName))
                                {
                                    Console.WriteLine("[WindowAction] OPEN: target shellId is empty.");
                                    return;
                                }
                                PrimaryShellLifecycleController.HandleWindowLifecycle(
                                    "OPEN",
                                    targetName,
                                    "",
                                    null,
                                    null
                                );
                                break;

                            case "CLOSE":
                                // CLOSE：windowUniqueId = 閉じたい個体ID、shellId = ""（不要）
                                if (string.IsNullOrWhiteSpace(targetName))
                                {
                                    Console.WriteLine("[WindowAction] CLOSE: target windowUniqueId is empty.");
                                    return;
                                }
                                PrimaryShellLifecycleController.HandleWindowLifecycle(
                                    "CLOSE",
                                    "",
                                    targetName,
                                    null,
                                    null
                                );
                                break;

                            case "RESTART":
                                // RESTART：shellId = 再起動で開き直す種別ID（必要）、windowUniqueId は要求元（任意）
                                if (string.IsNullOrWhiteSpace(targetName))
                                {
                                    Console.WriteLine("[WindowAction] RESTART: target shellId is empty.");
                                    return;
                                }
                                PrimaryShellLifecycleController.HandleWindowLifecycle(
                                    "RESTART",
                                    targetName,
                                    requestorWindowId,
                                    null,
                                    null
                                );
                                break;

                            case "SHUTDOWN":
                                // SHUTDOWN：必須なし（要求元は任意で渡す）
                                PrimaryShellLifecycleController.HandleWindowLifecycle(
                                    "SHUTDOWN",
                                    "",
                                    requestorWindowId,
                                    null,
                                    null
                                );
                                break;

                            default:
                                Console.WriteLine($"[WindowAction] Unknown command: {value.FirstCommand}");
                                break;
                        }
                    }

                }
            }
        }

        public void InitializeFromSetting(string windowUniqueId)
        {
            WindowUniqueId = windowUniqueId;

        }

        /// <summary>
        /// CompositeViewModelから選択通知を受けたときに呼ばれる初期化処理。
        /// </summary>
        public void OnSelected(string windowUniqueId, string elementId)
        {
            this.WindowUniqueId = windowUniqueId;
            CommandItems.Clear();

            var items = CommandItemBuilder.Build();
            foreach (var item in items)
            {
                CommandItems.Add(item);
            }
        }

        /// <summary>
        /// 単一のWindow制御コマンドを表すデータ構造。
        /// </summary>
        public class CommandItem
        {
            public string DisplayText { get; set; } = string.Empty;
            public string FirstCommand { get; set; } = string.Empty;
            public string FirstTargetName { get; set; } = string.Empty;
        }
    }
}