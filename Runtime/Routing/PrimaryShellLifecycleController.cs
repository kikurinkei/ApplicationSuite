using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.Runtime.Windowing.Close;

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
        public static bool HandleWindowLifecycle(
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
                                return false;
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
                            return true; 
                        }
                    case "CLOSE":
                        // 実務担当（Sequencer）を呼び出し、その結果をそのまま Shell へ return する
                        return ShellCloseSequencer.PrepareForShutdown(windowUniqueId);

                    case "RESTART":
                        {
                            if (string.IsNullOrWhiteSpace(shellId))
                            {
                                Console.WriteLine("[PrimaryShellLifecycleController] RESTART: shellId is empty.");
                                return false; 
                            }

                            Windowing.Close.RestartRequestProc.Process(status, shellId);
                            return true;
                            
                        }

                    case "SHUTDOWN":
                        {
                            Windowing.Close.ShutdownRequestProc.Execute(status);
                            return true;
                            
                        }

                    default:
                        {
                            Console.WriteLine($"[PrimaryShellLifecycleController] 未定義のステータス: {status}");
                            // 想定外のステータスも、ひとまず「処理継続OK」として true を返す
                            // （ここで false を返すと BaseShell が閉じられなくなるため安全側に倒す）
                            return true;
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
                // --- ここが重要！ ---
                // 例外が発生した（失敗した）ので、false を返して BaseShell に「閉じちゃダメだ」と伝えます
                return false;
            }
            // 万が一 switch を通り抜けた場合のための保険（通常はここには来ません）
            return true;
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
