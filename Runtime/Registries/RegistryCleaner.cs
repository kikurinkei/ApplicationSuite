using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.Runtime.Registries._Base;
using ApplicationSuite.Runtime.Windowing;

namespace ApplicationSuite.Runtime.Registries
{
    /// <summary>
    /// SHUTDOWN処理を行うユーティリティクラス（全レジストリを削除し、状態を確認）
    /// </summary>
    public static class RegistryCleaner
    {
        public static void ClearAll()
        {
            //ViewModelRegistry.Instance.ClearAll();
            //UserControlRegistry.Instance.ClearAll();
            //WindowRegistry.Instance.ClearAll();
            //CompositeViewModelRegistry.Instance.ClearAll();
            //NavigationListRegistry.Instance.ClearAll();
        }
        public static void ClearOne(string windowId)
        {
            ViewModelRegistry.Instance.RemoveAll(windowId);
            UserControlRegistry.Instance.RemoveAll(windowId);
            WindowRegistry.Instance.Remove(windowId);
            CompositeViewModelRegistry.Instance.Remove(windowId);
            NavigationListRegistry.Instance.Remove(windowId);
        }
        public static bool AreAllEmpty()
        {
            return ViewModelRegistry.Instance.IsEmpty()
                && UserControlRegistry.Instance.IsEmpty()
                && WindowRegistry.Instance.IsEmpty()
                && CompositeViewModelRegistry.Instance.IsEmpty()
                && NavigationListRegistry.Instance.IsEmpty();
        }
    }
}
