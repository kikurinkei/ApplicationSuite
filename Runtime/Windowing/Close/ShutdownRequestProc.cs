using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.Runtime.Windowing.Close
{
    public class ShutdownRequestProc
    {
        //public static void Execute(string status, string? restartShellId)
        public static void Execute(string status)
        {
            // 1. すべてのwindowUniqueIdを取得し、順に閉じる
            var windowIds = WindowRegistry.Instance.GetKeys();

            foreach (var id in windowIds.ToList()) // ToList()でコピー（途中変更対策）
            {
                // WindowCloseProcを使用してウィンドウを閉じる
                WindowCloseProc.Process(status, id);
            }
        }
    }
}
