using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.AppGenerator.Catalog;
using ApplicationSuite.Runtime.Registries;
using ApplicationSuite.WindowModules.AppShared.Utilities.Logging;
namespace ApplicationSuite.AppGenerator.Activation
{
    /// <summary>
    /// UserControlBuilder の責務は、構成情報から UserControl を生成し、
    /// UserControlRegistry に登録することです。
    /// </summary>
    public static class UserControlBuilder
    {
        /// <summary>
        /// 指定された ID 群から UserControl を生成・登録します。
        /// </summary>
        public static void BuildAndRegister(string windowUniqueId, Dictionary<string, UtilityMetaInfo> childIds)
        {
            foreach (var kv in childIds)
            {
                // 1. ControlPath を取得
                string eId = kv.Value.UIElementId; // 修正: KeyValuePair の Value プロパティを使用
                string? ucPath = kv.Value.UIControlPath; // 修正: KeyValuePair の Value プロパティを使用

                if (string.IsNullOrEmpty(ucPath))
                {
                    Console.WriteLine($"[UserControlBuilder][Skip] ControlPath null: {kv.Value.UIElementId}");
                    continue;
                }
                // 2. 型解決
                var ucType = Type.GetType(ucPath);
                if (ucType == null)
                {
                    Console.WriteLine($"[UserControlBuilder][Error] 型が見つかりません: {ucPath}");
                    LogHandler.Handle("UserControlBuilder", "UC001", "UserControl", $"型が見つかりません: {ucPath}");
                    continue;
                }
                // 3. インスタンス生成
                var instance = Activator.CreateInstance(ucType);
                if (instance == null)
                {
                    Console.WriteLine($"[UserControlBuilder][Error] インスタンス生成失敗: {ucPath}");
                    LogHandler.Handle("UserControlBuilder", "UC002", "UserControl", $"インスタンス生成失敗: {ucPath}");
                    continue;
                }
                // 4. レジストリ登録
                UserControlRegistry.Instance.Register(windowUniqueId, eId, instance);
                Console.WriteLine($"[UserControlBuilder][OK] 登録成功: {ucPath}");
                LogHandler.Handle("UserControlBuilder", "UC003", "UserControl", $"インスタンス登録成功: {ucPath}");
            }
        }
    }
}