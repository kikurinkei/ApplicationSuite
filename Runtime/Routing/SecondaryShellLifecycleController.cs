using System;
using ApplicationSuite.Runtime.Windowing;
using ApplicationSuite.Runtime.Registries;
using ApplicationSuite.WindowModules.AppShared.Base;

namespace ApplicationSuite.Runtime.Service.Routing
{
    /// <summary>
    /// Secondary のライフサイクルと、Primary→Secondary Manual の“ジャンプ橋渡し”。
    /// </summary>
    public class SecondaryShellLifecycleController
    {
        // 統一契約：
        // HandleWindowLifecycle(status, shellId, windowUniqueId, parentWindowId?, parentSelectedElementId?)
        public static void HandleWindowLifecycle(
            string status,
            string shellId,
            string windowUniqueId,
            string? parentWindowId,
            string? parentSelectedElementId
        )
        {
            try
            {
                switch (status)
                {
                    case "OPEN":
                        {
                            // TODO
                            // OPEN：shellId 必須 / windowUniqueId は未指定ならここで生成
                            if (string.IsNullOrWhiteSpace(shellId))
                            {
                                Console.WriteLine("[SecondaryShellLifecycleController] OPEN: shellId is empty.");
                                return;
                            }

                            var actualWindowUniqueId =
                                string.IsNullOrWhiteSpace(windowUniqueId)
                                    ? GenerateWindowId.GetWindowUniqueId(shellId)
                                    : windowUniqueId;

                            // 親情報は OPEN 時のみ意味を持つ（未指定なら空文字で下流へ）
                            var parentId = parentWindowId ?? string.Empty;
                            var elementId = parentSelectedElementId ?? string.Empty;

                            SecondaryShellBuildPipeline.Process(
                                shellId,
                                actualWindowUniqueId,
                                parentId,
                                elementId
                                );

                            //Bridge.DeliverBuffered(actualWindowUniqueId);
                        }
                        break;

                    case "CLOSE":
                        {
                            // CLOSE：windowUniqueId 必須（shellId を閉じる対象に流用しない）
                            if (string.IsNullOrWhiteSpace(windowUniqueId))
                            {
                                Console.WriteLine("[SecondaryShellLifecycleController] CLOSE: windowUniqueId is empty.");
                                return;
                            }

                            WindowRegistry.Instance.UnlinkByChild(windowUniqueId);
                            Windowing.Close.WindowCloseProc.Process(status, windowUniqueId);
                        }
                        break;

                    case "RESTART":
                        {
                            // RESTART：現状仕様に合わせ、shellId を必須（再OPEN対象の種別ID）
                            if (string.IsNullOrWhiteSpace(shellId))
                            {
                                Console.WriteLine("[SecondaryShellLifecycleController] RESTART: shellId is empty.");
                                return;
                            }

                            Windowing.Close.RestartRequestProc.Process(status, shellId);
                        }
                        break;

                    case "SHUTDOWN":
                        Windowing.Close.ShutdownRequestProc.Execute(status);
                        break;

                    default:
                        Console.WriteLine("[SecondaryShellLifecycleController] 未定義ステータス: " + status);
                        break;
                }
            }
            catch (Exception ex)
            {
                var categoryCore = !string.IsNullOrWhiteSpace(shellId)
                    ? shellId
                    : (!string.IsNullOrWhiteSpace(windowUniqueId) ? windowUniqueId : "UNKNOWN");

                WindowModules.AppShared.Utilities.Logging.LogHandler.Handle(
                    className: nameof(SecondaryShellLifecycleController),
                    identifier: "Error",
                    category: $"-{categoryCore}--",
                    message: ex.Message
                );
            }
        }
    }

    /// <summary>
    /// Primary → Secondary 間の“アンカージャンプ”指令の橋渡し。
    /// 役割：
    ///   - Jump(...)         : Primary（親）から届いた elementId を、対応する Secondary（子）の ManualViewVM へ届ける。
    ///                         まだ Secondary 側の AVM が準備できていない場合は、一旦バッファに溜める。
    ///   - DeliverBuffered() : Secondary ウィンドウが OPEN 後（AVM 構築後）に、溜めておいた elementId を配達する。
    ///   - TryOnSelected...  : Secondary の AVM から ManualViewVM を取得し、ISelectedAware.OnSelected を呼び出す。
    ///
    /// 呼び出しタイミング（典型）：
    ///   1) Primary 側で「Manual を開く」操作 → SecondaryShellLifecycleController.HandleWindowLifecycle("OPEN", ...)
    ///   2) Secondary のビルド完了後に DeliverBuffered(secondaryWindowId) が呼ばれ、保留分があれば配達される。
    ///
    /// 注意：
    ///   - 本クラスは“薄い仲介”のみ行い、UI スレッド切替や例外ダイアログ等は行わない。
    ///   - _pending は static Dictionary。マルチスレッドで同時に Jump が走る設計であれば、排他の検討が必要。
    ///     （現状：UI 操作直列前提の最小実装）
    /// </summary>
    public static class Bridge
    {
        // secondaryWindowId → last elementId（AVM 未準備時に直近 1 件だけ保持）
        // ・Secondary 側の AVM（ManualViewVM）がまだ生成されていないとき、
        //   ここに elementId を一時保存する。ウィンドウ構築後 DeliverBuffered で回収・配達する。
        private static readonly Dictionary<string, string> _pending
            = new Dictionary<string, string>();

        /// <summary>
        /// Primary（親）→ Secondary（子）への“ジャンプ指令”を発火する。
        /// - 親 child 関係は WindowRegistry で管理されている想定。
        /// - 子ウィンドウが見つからない／まだ AVM 未準備のときはバッファに積む。
        /// </summary>
        /// <param name="primaryWindowId">親（Primary）ウィンドウの WindowUniqueId</param>
        /// <param name="elementId">ジャンプ先アンカーID（例："Dashboard"）</param>
        public static void Jump(string primaryWindowId, string elementId)
        {
            // 引数チェック（elementId 未指定は無視）
            if (string.IsNullOrEmpty(primaryWindowId) || string.IsNullOrEmpty(elementId))
            {
                Console.WriteLine("[Bridge.Jump] 引数不正");
                return;
            }

            // 親→子 の対応を WindowRegistry から取得
            string secondaryWindowId;
            if (!WindowRegistry.Instance.TryGetChildOfParent(primaryWindowId, out secondaryWindowId) ||
                string.IsNullOrEmpty(secondaryWindowId))
            {
                // 子ウィンドウが未生成・未登録のケース（OPEN 前やクローズ後）
                Console.WriteLine("[Bridge.Jump] Child not found (parent=" + primaryWindowId + ")");
                return;
            }

            // すでに Secondary 側の AVM が準備できていれば、即時配達を試みる
            if (TryOnSelectedManual(secondaryWindowId, elementId))
            {
                Console.WriteLine("[Bridge.Jump] Delivered: " + secondaryWindowId + " ← " + elementId);
                return;
            }

            // AVM 未準備（または ManualViewVM 未登録）の場合は、後配達用にバッファリング
            _pending[secondaryWindowId] = elementId;
            Console.WriteLine("[Bridge.Jump] Buffered (secondary=" + secondaryWindowId + ", id=" + elementId + ")");
        }

        /// <summary>
        /// バッファに溜めた elementId を、Secondary 側の AVM 構築完了後に配達する。
        /// - SecondaryShellBuildPipeline 完了直後に呼び出される想定。
        /// - 一度配達できたらバッファから削除する。
        /// </summary>
        /// <param name="secondaryWindowId">子（Secondary）ウィンドウの WindowUniqueId</param>
        public static void DeliverBuffered(string secondaryWindowId)
        {
            if (string.IsNullOrEmpty(secondaryWindowId)) return;

            string elementId;
            // secondaryWindowId に対応する保留指令があるか？
            if (_pending.TryGetValue(secondaryWindowId, out elementId))
            {
                // いまなら ManualViewVM が取得できるはずなので再試行
                if (TryOnSelectedManual(secondaryWindowId, elementId))
                {
                    Console.WriteLine("[Bridge] DeliverBuffered OK: " + secondaryWindowId + " ← " + elementId);
                    _pending.Remove(secondaryWindowId); // 配達できたので削除
                }
                // まだ取得できない場合は、ここでは残す（再度 DeliverBuffered を呼べばよい設計）
            }
        }

        /// <summary>
        /// Secondary の CompositeViewModel を取り出し、"ManualView" の VM を探して
        /// ISelectedAware.OnSelected(secondaryWindowId, elementId) を呼ぶ。
        ///
        /// ポイント：
        ///   - 追加の IF を増やさず、既存の ISelectedAware を“標準の受け口”として活用する。
        ///   - AVM["ManualView"] → BaseViewModel → ISelectedAware へ明示キャストして呼び出す。
        ///   - 成功時 true／未準備・未登録時 false を返し、上位（Jump/DeliverBuffered）で次の手を判断する。
        /// </summary>
        private static bool TryOnSelectedManual(string secondaryWindowId, string elementId)
        {
            // 1) Secondary 側の Composite を取得（未構築なら null）
            var composite = CompositeViewModelRegistry.Instance.Get(secondaryWindowId);
            if (composite == null) return false;

            // 2) AVM（要素名→VM）から "ManualView" を引く
            BaseViewModel vm;
            if (composite.AVM != null && composite.AVM.TryGetValue("ManualView", out vm))
            {
                // 3) 既定受け口 ISelectedAware に明示キャスト（パターン変数は使わず互換性優先）
                var aware = vm as ISelectedAware;
                if (aware != null)
                {
                    // 4) ManualViewVM へ「この anchorId に移動して」を通知
                    //    - aware 実装側で PendingAnchorId へ格納し、AutoScroll 設定に応じて即時解決される想定
                    aware.OnSelected(secondaryWindowId, elementId);
                    return true;
                }
            }

            // "ManualView" がまだ AVM に乗っていない／型不一致などで失敗
            return false;
        }
    }

}

