using ApplicationSuite.Runtime.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.Runtime.Service
{
    public static class WindowSyncBridge
    {
        public static void PushTransferContext(
            string ParentWindowUniqueId, string parentSelectedElementId)
        {
            // WindowRegistryに自らのWindowUniqueIdを持つSecondaryShellが存在するか確認
            // 存在るのなら、secondaryWindowUniqueIdを取得
            string secondaryWindowUniqueId;
            if (!WindowRegistry.Instance.TryGetChildOfParent(
                　　ParentWindowUniqueId, out secondaryWindowUniqueId) ||
                string.IsNullOrEmpty(secondaryWindowUniqueId))
            {
                // 存在する場合は、ManualViewViewModelのセッターを直接呼び出す

            }
            else
            {
                // 存在しない場合は、TransferMapに登録。
                // （SecondaryShellがOPENされた際に、TransferMapから取得できるようにする
                HoldTransferPayload(
                    ParentWindowUniqueId, parentSelectedElementId);
            }
        }
        // TransferMapに登録
        public static void HoldTransferPayload(
            string ParentWindowUniqueId, string parentSelectedElementId)
        {
            ParentWindowElementIdCache.Instance.AddOrUpdate(
                ParentWindowUniqueId, parentSelectedElementId);
        }
    }
}
