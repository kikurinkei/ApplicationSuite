using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.AppGenerator.Configuration;
using ApplicationSuite.WindowModules.AppShared.Base;


namespace ApplicationSuite.Runtime.Windowing
{
    /// <summary>
    /// WindowBuilder の責務は、構成情報から Window を生成し、WindowRegistry に登録することです。
    /// </summary>
    public static class WindowBuilder
    {
        /// <summary>
        /// 指定された shellId に対応する Window を構築・登録します。
        /// </summary>
        public static void Build(string Status, string windowUniqueId, string shellId)
        {
            // 1. ControlPath を取得
            string? path = ElementPropertyAccessor.GetStringProperty("shell", shellId, "ControlPath");
            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine($"[WindowBuilder][Skip] ControlPath null: {shellId}");
                return;
            }
            // 2. 型解決
            var winType = Type.GetType(path);
            if (winType == null)
            {
                Console.WriteLine($"[WindowBuilder][Error] 型が見つかりません: {path}");
                return;
            }

            // 既存の var instance = null; を以下のように修正
            //object? instance = null;


            switch (Status) 
            {

                case "Primary":
                    // 3. インスタンス生成
                     var PrimaryBaseShellInstance = Activator.CreateInstance(winType)  as IShell;
                    if (PrimaryBaseShellInstance == null)
                    {
                        Console.WriteLine($"[WindowBuilder][Error] インスタンス生成失敗: {path}");
                        return;
                    }

                    // object? instance = PrimaryBaseShellInstance;

                    // 4. 登録
                    WindowRegistry.Instance.Register(windowUniqueId, PrimaryBaseShellInstance);

                    break;
                case "Secondary":
                    // 3. インスタンス生成
                    var SecondaryBaseShellInstance = Activator.CreateInstance(winType)  as IShell;

                    if (SecondaryBaseShellInstance == null)
                    {
                        Console.WriteLine($"[WindowBuilder][Error] インスタンス生成失敗: {path}");
                        return;
                    }
                    // 4. 登録
                    WindowRegistry.Instance.Register(windowUniqueId, SecondaryBaseShellInstance);
                    break;
                default:
                    Console.WriteLine($"[WindowBuilder][Error] 未知のステータス: {Status}");
                    return;
            }



            //// 5.　RegistryCleanProcess
            //instance.NotifyClosedExternally += RegistryCleanProc.registryCleanProc;

            Console.WriteLine($"[WindowBuilder][OK] 登録成功: {path}");
        }


    }
}