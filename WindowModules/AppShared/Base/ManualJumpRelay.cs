using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ApplicationSuite.Runtime.Registries;

// ROLE: PairingHub から呼ばれる「配達係」。
//       childWindowUniqueId で SecondaryCompositeViewModel を Registry から引き、
//       同期ON/OFF と 親子一致をチェックしてから、AVM へ委譲する。
// NOTE: 最小実装（理解優先）
//       - ここではログ基盤には繋がない（TODO コメントだけ残す）

namespace ApplicationSuite.WindowModules.AppShared.Base
{
    /// <summary>
    /// ManualJumpRelay（最小 / Registry解決版）
    /// </summary>
    public class ManualJumpRelay : IManualJumpRelay
    {
        public void Deliver(string childWindowUniqueId, string parentId, string elementId)
        {
            if (string.IsNullOrEmpty(childWindowUniqueId)) return;
            if (string.IsNullOrEmpty(parentId)) return;
            if (string.IsNullOrEmpty(elementId)) return;

            // 1) childWindowUniqueId から SecondaryCompositeViewModel を引く
            //    ※このアプリの大原則：必ず Registry を経て使う
            var scvm = CompositeViewModelRegistry.Instance.Get<SecondaryCompositeViewModel>(childWindowUniqueId);
            if (scvm == null)
            {
                // TODO: ログ（今は Immediate Window 想定）
                Debug.WriteLine($"[Pairing][Relay] SecondaryCompositeVM not found. child={childWindowUniqueId}");
                return;
            }

            // 2) 同期ON/OFF（null は false 扱い）
            if (scvm.IsRealtimeSyncEnabled != true)
            {
                return;
            }

            // 3) 親子一致チェック（誤配達ガード）
            if (!string.Equals(scvm.PairedWindowUniqueId, parentId, StringComparison.Ordinal))
            {
                // TODO: ログ（今は Immediate Window 想定）
                Debug.WriteLine($"[Pairing][Relay] Parent mismatch. child={childWindowUniqueId}, expectedParent={scvm.PairedWindowUniqueId}, gotParent={parentId}");
                return;
            }

            // 4) ここから先は SecondaryCompositeVM（＝内部コンポーネント）の責務
            //    まずは「表示中AVMに委譲できるなら委譲」→ ダメなら何もしない。
            var avm = scvm.CurrentContentViewModel;

            if (avm is IPairedSelectionReceiver receiver)
            {
                receiver.ReceiveSelection(parentId, elementId);
                return;
            }

            // 5) （任意）elementId が「AVMのキー」だった場合は、画面切替として解釈できる。
            //    ※いまは最小実装なので、切替も"試すだけ"に留める。
            //    ※不要ならコメントアウトしてOK。
            var vm = scvm.Resolve(elementId);
            if (vm != null)
            {
                scvm.CurrentContentViewModel = vm;
            }

            // TODO: ここで "手動マニュアル" のスクロール等に繋ぐ場合は、
            //       Secondary側（または AVM）に統一口（ReceiveSelection）を作って集約する。
        }
    }

    /// <summary>
    /// AVM（Active ViewModel）が「ペアリングからの選択」を受け取れる場合の最小I/F。
    /// - 実装先の例：ManualViewViewModel / ManualLinkViewModel など
    /// </summary>
    public interface IPairedSelectionReceiver
    {
        void ReceiveSelection(string parentId, string elementId);
    }
}
