using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.Runtime.Service.Routing
{
    public static class PrimaryShellLifecycleController
    {
        /// <summary>
        /// 統一契約：
        /// HandleWindowLifecycle(status, shellId, windowUniqueId, parentWindowId?, parentSelectedElementId?)
        ///
        /// ルール（最低限）：
        /// - OPEN    : shellId 必須 / windowUniqueId は空でも可（空なら生成）
        /// - CLOSE   : windowUniqueId 必須 / shellId は空でも可
        /// - RESTART : shellId 必須（再OPEN対象）
        /// - SHUTDOWN: 必須なし
        /// </summary>
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
                            if (string.IsNullOrWhiteSpace(shellId))
                            {
                                Console.WriteLine("[PrimaryShellLifecycleController] OPEN: shellId is empty.");
                                return;
                            }

                            // OPEN では windowUniqueId 未指定ならここで生成
                            var actualWindowUniqueId =
                                string.IsNullOrWhiteSpace(windowUniqueId)
                                    ? GenerateWindowId.GetWindowUniqueId(shellId)
                                    : windowUniqueId;

                            PrimaryShellBuildPipeline.Process(
                                shellId,
                                actualWindowUniqueId,
                                null,
                                null
                                );
                            break;
                        }

                    case "CLOSE":
                        {
                            // CLOSE は windowUniqueId が閉じる対象（shellId を流用しない）
                            if (string.IsNullOrWhiteSpace(windowUniqueId))
                            {
                                Console.WriteLine("[PrimaryShellLifecycleController] CLOSE: windowUniqueId is empty.");
                                return;
                            }

                            Windowing.Close.WindowCloseProc.Process(status, windowUniqueId);
                            break;
                        }

                    case "RESTART":
                        {
                            if (string.IsNullOrWhiteSpace(shellId))
                            {
                                Console.WriteLine("[PrimaryShellLifecycleController] RESTART: shellId is empty.");
                                return;
                            }

                            Windowing.Close.RestartRequestProc.Process(status, shellId);
                            break;
                        }

                    case "SHUTDOWN":
                        {
                            Windowing.Close.ShutdownRequestProc.Execute(status);
                            break;
                        }

                    default:
                        {
                            Console.WriteLine($"[PrimaryShellLifecycleController] 未定義のステータス: {status}");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                // ※元コードの「わざと例外を投げる」は削除（ログがノイズ化するだけで害が大きい）
                var categoryCore = !string.IsNullOrWhiteSpace(shellId)
                    ? shellId
                    : (!string.IsNullOrWhiteSpace(windowUniqueId) ? windowUniqueId : "UNKNOWN");

                WindowModules.AppShared.Utilities.Logging.LogHandler.Handle(
                    className: nameof(PrimaryShellLifecycleController),
                    identifier: "Error",
                    category: $"-{categoryCore}--",
                    message: ex.Message
                );
            }
        }
    }
}



//namespace ApplicationSuite.Runtime.Service.Routing
//{
//    public static class PrimaryShellLifecycleController
//    {
//        public static void HandleWindowLifecycle(string status, string shellId, string WindowUniqueId)
//        {
//            switch (status)
//            {
//                case "OPEN":
//                    PrimaryShellBuildPipeline.Process(
//                        shellId, GenerateWindowId.GetWindowUniqueId(shellId));
//                    break;

//                case "CLOSE":
//                    Windowing.Close.WindowCloseProc.Process(
//                        status, shellId); // ここのshellIdは、閉じるウィンドウのID
//                    break;

//                case "RESTART":
//                    Windowing.Close.RestartRequestProc.Process(
//                        status, shellId);
//                    break;

//                case "SHUTDOWN":
//                    Windowing.Close.ShutdownRequestProc.Execute(
//                        status);
//                    break;

//                default:
//                    // 未定義のステータスの場合は何も行わない
//                    Console.WriteLine($"[WindowLifecycleController] 未定義のステータス: {status}");
//                    break;
//            }
//            try
//            {
//                throw new Exception("エラーが発生しました");
//            }
//            catch (Exception ex)
//            {
//                WindowModules.AppShared.Utilities.Logging.
//                LogHandler.Handle(
//                    className: nameof(PrimaryShellLifecycleController),
//                    identifier: "Error",
//                    category: $"-{shellId}--", // ← UserControlのフィルタ条件と一致
//                    message: ex.Message
//                    );

//            }

//            //// レジストリのクリア処理
//            //// 全てのレジストリが空であればアプリケーションを終了
//            //if (RegistryCleaner.AreAllEmpty())
//            //{
//            //    Application.Current.Shutdown();
//            //}
//        }
//    }
//}
