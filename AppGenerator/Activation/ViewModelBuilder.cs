using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.AppGenerator.Catalog;
using ApplicationSuite.Runtime.Registries;
using ApplicationSuite.WindowModules.AppShared.Base;
using ApplicationSuite.WindowModules.AppShared.Utilities.Logging;

namespace ApplicationSuite.AppGenerator.Activation
{
    /// <summary>
    /// ViewModelBuilder の責務は、構成情報から ViewModel を生成し、
    /// ViewModelRegistry に登録することです。
    /// </summary>
    public static class ViewModelBuilder
    {
        /// <summary>
        /// 指定された ID 群から ViewModel を生成・登録します。
        /// </summary>
        public static void BuildAndRegister(string windowUniqueId, Dictionary<string, UtilityMetaInfo> childIds)
        {
            foreach (var kv in childIds)
            {
                // 1. ViewModelPath を取得
                string eId = kv.Value.UIElementId;
                //string eId = elementId; // 修正: KeyValuePair の Value プロパティを使用
                //string eId = elementId.Value.ElementId; // 修正: KeyValuePair の Value プロパティを使用
                string? vmPath = kv.Value.UIViewModelPath; // 修正: KeyValuePair の Value プロパティを使用

                if (string.IsNullOrEmpty(vmPath))
                {
                    Console.WriteLine($"[ViewModelBuilder][Skip] ViewModelPath null: {eId}");
                    continue;
                }
                // 2. 型解決
                var vmType = Type.GetType(vmPath);
                if (vmType == null)
                {
                    Console.WriteLine($"[ViewModelBuilder][Error] 型が見つかりません: {vmPath}");
                    LogHandler.Handle("ViewModelBuilder", "VM001", "ViewModel", $"型が見つかりません: {vmPath}");
                    continue;
                }
                // 3. インスタンス生成
                var instance = Activator.CreateInstance(vmType) as BaseViewModel;
                if (instance == null)
                {
                    Console.WriteLine($"[ViewModelBuilder][Error] インスタンス生成失敗: {vmPath}");
                    LogHandler.Handle("ViewModelBuilder", "VM002", "ViewModel", $"インスタンス生成失敗: {vmPath}");
                    continue;
                }
                // 4. レジストリ登録
                ViewModelRegistry.Instance.Register(windowUniqueId, eId, instance);
                Console.WriteLine($"[ViewModelBuilder][OK] 登録成功: {vmPath}");
                LogHandler.Handle("ViewModelBuilder", "VM003", "ViewModel", $"インスタンス登録成功: {vmPath}");
            }
        }
    }
}
