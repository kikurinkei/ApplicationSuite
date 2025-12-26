using ApplicationSuite.Runtime.Registries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ROLE: 「入口」— Publish / Register / Unregister を受け取り、
//       PairingRegistry に保存し、可能なら ManualJumpRelay に配達する。
// NOTE: 最小実装（理解優先）
//       - Hub は Singleton にしない（生成/注入は呼び出し側の責務）
//       - 余計な状態（LastDelivered 等）は持たない
//       - Relay が未設定なら何もしない

namespace ApplicationSuite.WindowModules.AppShared.Base
{
    /// <summary>
    /// PairingHub（最小）
    /// </summary>
    public class PairingHub
    {
        // 配達係（未設定なら何もしない）
        public IManualJumpRelay? ManualJumpRelay { get; set; }

        /// <summary>
        /// Primary で選択が変わった（= latest 更新）。
        /// child が居れば、その child に配達する。
        /// </summary>
        public void PublishSelection(string parentId, string elementId)
        {
            if (string.IsNullOrEmpty(parentId)) return;
            if (string.IsNullOrEmpty(elementId)) return;

            // 1) 保存（latest は常に上書き）
            PairingRegistry.Instance.SetLatest(parentId, elementId);

            // 2) child が居れば配達
            var childWindowUniqueId = PairingRegistry.Instance.GetChildOrNull(parentId);
            if (string.IsNullOrEmpty(childWindowUniqueId)) return;

            Deliver(childWindowUniqueId, parentId, elementId);
        }

        /// <summary>
        /// Secondary が parent に紐づいた（= child 登録）。
        /// latest が既にあれば、その child に配達する。
        /// </summary>
        public void RegisterChild(string parentId, string childWindowUniqueId)
        {
            if (string.IsNullOrEmpty(parentId)) return;
            if (string.IsNullOrEmpty(childWindowUniqueId)) return;

            // 1) 登録（parent あたり 1 child の前提なので上書き）
            PairingRegistry.Instance.Register(parentId, childWindowUniqueId);

            // 2) latest があれば配達
            var elementId = PairingRegistry.Instance.GetLatestOrNull(parentId);
            if (string.IsNullOrEmpty(elementId)) return;

            Deliver(childWindowUniqueId, parentId, elementId);
        }

        /// <summary>
        /// Secondary が閉じた等で child を解除（childId 起点）。
        /// </summary>
        public void UnregisterChild(string childWindowUniqueId)
        {
            if (string.IsNullOrEmpty(childWindowUniqueId)) return;

            PairingRegistry.Instance.UnregisterByChild(childWindowUniqueId);
        }

        /// <summary>
        /// Primary が閉じた等で parent を丸ごと掃除。
        /// </summary>
        public void OnParentClosed(string parentId)
        {
            if (string.IsNullOrEmpty(parentId)) return;

            PairingRegistry.Instance.UnregisterParent(parentId);
        }

        private void Deliver(string childWindowUniqueId, string parentId, string elementId)
        {
            // Relay 未設定なら何もしない（まずコンパイル優先）
            if (ManualJumpRelay == null) return;

            ManualJumpRelay.Deliver(childWindowUniqueId, parentId, elementId);
        }
    }

    /// <summary>
    /// ManualJumpRelay（インターフェース）
    /// - childWindowUniqueId と parentId と elementId を受け取り、実際の受け口へジャンプさせる。
    /// </summary>
    public interface IManualJumpRelay
    {
        void Deliver(string childWindowUniqueId, string parentId, string elementId);
    }
}