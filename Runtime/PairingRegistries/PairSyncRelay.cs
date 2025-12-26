using ApplicationSuite.Runtime.Pairing;
using ApplicationSuite.Runtime.Registries;
using ApplicationSuite.WindowModules.AppShared.Base;
using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ApplicationSuite.WindowModules.SecondaryWindow.ManualView;

namespace ApplicationSuite.Runtime.Pairing
{
    public class PairSyncRelay
    {
        // ParentIds フィールドを static に変更
        private static List<string> ParentIds = new List<string>();

        // SecondaryWindowPropertyInjector用
        // レジストリ登録だけ。
        // 使わないのでコメントアウト
        //public static void RegisterPair(string parentId, string childId, bool SyncFlig)
        //{
        //    // PairRegistry に登録
        //    // PairRegistry.Instance.Register(parentId, childId, SyncFlig);
        //}

        //　Primary側への受け口
        public static void SetElementId(string parentId, string childId, string _elementId)
        {
            var vm = ViewModelRegistry.Instance.Get(childId, "ManualView") as ManualViewViewModel;
            if (vm != null)
            {
                vm.AnchorId = _elementId;
            }
        }
        //　SecondaryCompositeViewModel用の受け口
        public static string GetElementId(string parentId, string childId, bool bitFlug)
        {
            string _elementId = string.Empty;
            // bitFlug の意味
            // 0 real time mode Off
            // 1 real time mode On

            // ParentIds に parentId　が含まれていない場合、且つ bitFlug が 0 の場合。
            // LIST = なにもしない。
            // 手動取得モード
            if (!ParentIds.Contains(parentId) && bitFlug == false)
            {
                //ここには到達しない。
                return null;
            }
            // ParentIds に parentId　が含まれていない場合、且つ bitFlug が 1 の場合。
            // ParentIds に parentId をセット
            // PrimaryCompositeViewModel に RealtimeSync を true にセット
            // 自動取得モード
            else if (!ParentIds.Contains(parentId) && bitFlug == true)
            {
                // ParentIds に parentId をセット
                ParentIds.Add(parentId);

                // PrimaryCompositeViewModel に RealtimeSync を true にセット/ ElementId を取得
                _elementId = SetFlugAndGetElementId(parentId);

                // 取得した ElementId をApplyElementId経由で返す渡す。
                return _elementId;

            }
            // ParentIds に parentId　が含まれている場合、且つ bitFlug が 1 の場合。
            // LIST = なにもしない。
            // 自動取得モード継続
            else if (ParentIds.Contains(parentId) && bitFlug == true)
            {
                //ここには到達しない。
                return null;
            }
            // ParentIds に parentId　が含まれている場合、且つ bitFlug が 0 の場合。
            // ParentIds から parentId を削除
            // 手動取得モードに戻す。
            else if (ParentIds.Contains(parentId) && bitFlug == false)
            {
                // ParentIds から parentId を削除
                ParentIds.Remove(parentId);
                // PrimaryCompositeViewModel に RealtimeSync を false にセット
                IsRealtimeSyncOff(parentId);
                // nullを返す。
                return null;
            }
            else
            {
                // それ以外はなにもしない。
                return null;
            }
        }

        public static string GetElementId(string parentId)
        {
            // childId から PrimaryCompositeViewModel を取得
            var _PrimaryCompositeViewModel = CompositeViewModelRegistry.Instance.Get(parentId) as PrimaryCompositeViewModel;
            // Composite.AVM["ManualView"];
            string _elementId = _PrimaryCompositeViewModel.CCVM;

            return _elementId;

        }

        //Primary側のスイッチとONへの変更、Primary側のElementId取得
        public static string SetFlugAndGetElementId(string parentId)
        {
            // PrimaryCompositeViewModel に RealtimeSync を true にセット
            var _PrimaryCompositeViewModel = CompositeViewModelRegistry.Instance.Get(parentId) as PrimaryCompositeViewModel;
            if (_PrimaryCompositeViewModel != null)
            {
                _PrimaryCompositeViewModel.IsRealtimeSyncEnabled = true;

                return _PrimaryCompositeViewModel.CCVM ?? string.Empty;
            }
            return string.Empty;
        }

        public static void IsRealtimeSyncOff(string parentId)
        {
            // PrimaryCompositeViewModel に RealtimeSync を false にセット
            var _PrimaryCompositeViewModel = CompositeViewModelRegistry.Instance.Get(parentId) as PrimaryCompositeViewModel;
            if (_PrimaryCompositeViewModel != null)
            {
                _PrimaryCompositeViewModel.IsRealtimeSyncEnabled = false;
            }
        }
    }

}
