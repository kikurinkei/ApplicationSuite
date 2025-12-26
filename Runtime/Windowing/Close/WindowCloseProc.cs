using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.WindowModules.AppShared.Base;
using ApplicationSuite.WindowModules.AppShared.Utilities.Logging;

namespace ApplicationSuite.Runtime.Windowing.Close
{
    public static class WindowCloseProc
    {
        public static void Process(string status, string WindowUniqueId) // ここのWindowUniqueIdは、閉じるウィンドウのID
        {
            // ログ記録
            LogHandler.Handle("WindowCloseProc", "CLOSE", "Lifecycle", $"Window閉鎖要求: {WindowUniqueId}");

            // 対象ウィンドウのインスタンスを取得し、明示的にClose処理を行う
            var window = WindowRegistry.Instance.Get(WindowUniqueId) as IShell;

            //　フラグを確認して、閉鎖処理を行う
            if (window != null && window.IsClosingInProgress == false)
            {
                //ウインドウの閉鎖が進行中でない場合、閉鎖フラグを立てる
                window.IsClosingInProgress = true;

                if (status == "RESTART")
                {
                    // RESTARTの場合は、IsRestartRequestedフラグを立てる
                    window.IsRestartRequested = true;
                }

                // ウィンドウを閉じる処理を実行
                window.Close();
            }

            //// すべてのwindowUniqueIdを取得し、表示する。
            //var windowIds = WindowRegistry.Instance.GetKeys();

            //foreach (var _items in windowIds.ToList()) // ToList()でコピー（途中変更対策）
            //{

            //    Console.WriteLine($"[-- Print windowIds ---]: {_items}");
            //}



        }
    }
}