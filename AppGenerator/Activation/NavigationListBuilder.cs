using ApplicationSuite.AppGenerator.Activation.Models;
using ApplicationSuite.AppGenerator.Catalog;
using ApplicationSuite.Runtime.Registries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.AppGenerator.Activation
{
    /// <summary>
    /// NavigationListBuilder の責務は、ElementDetail から NavigationListItem を構築し、
    /// NavigationListRegistry に登録することです。
    /// </summary>
    public static class NavigationListBuilder
    {
        /// <summary>
        /// 指定された ID 群から NavigationListItem を生成・登録します。
        /// </summary>
        //public static bool Build(string windowUniqueId, List<string> childIds)
        public static bool BuildAndRegister(string windowUniqueId, Dictionary<string, UtilityMetaInfo> childIds)
        {
            //var allDetails = ConfigStore.GetUtilityElements();
            var items = new List<NavigationListItem>();

            //foreach (var id in childIds)

            foreach (var kv in childIds)
            {

                string id = kv.Value.UIElementId; // 修正: KeyValuePair の Value プロパティを使用
                items.Add(new NavigationListItem
                {
                    ElementId = kv.Value.UIElementId,
                    ElementName = kv.Value.UIElementName, //.ElementName,
                    IconPath = kv.Value.UIIconPath
                });



            }

            NavigationListRegistry.Instance.Register(windowUniqueId, items);
            Console.WriteLine($"[NavigationListBuilder][OK] 登録成功: {items.Count} items");
            return true;
        }
    }
}