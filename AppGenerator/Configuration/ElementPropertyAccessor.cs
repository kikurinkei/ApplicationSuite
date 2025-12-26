using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.AppGenerator.Configuration
{
    public static class ElementPropertyAccessor
    {
        /// <summary>
        /// 指定した層（Suite / Shell / Utility）、要素ID、プロパティ名から
        /// ElementDetail の string プロパティを取得します。
        /// </summary>
        public static string? GetStringProperty(string layerName, string elementId, string propertyName)
        {
            Dictionary<string, ElementDetail>? storeLayerElements = null;
            // 対応する辞書（ConfigStore内）を取得
            switch (layerName.ToLowerInvariant())
            {
                case "suite":
                    layerName = "Suite";
                    storeLayerElements = ConfigStore.GetSuiteElements();
                    break;
                case "shell":
                    layerName = "Shell";
                    storeLayerElements = ConfigStore.GetShellElements();
                    break;
                case "utility":
                    layerName = "Utility";
                    storeLayerElements = ConfigStore.GetUtilityElements();
                    break;
                default:
                    return null; // 無効な層名
            }

            if (storeLayerElements == null) return null;

            // ElementDetail が存在するか確認
            if (storeLayerElements.TryGetValue(elementId, out var element))
            {
                // 指定されたプロパティを取得
                var propInfo = typeof(ElementDetail).GetProperty(propertyName);
                if (propInfo != null && propInfo.PropertyType == typeof(string))
                {
                    return propInfo.GetValue(element) as string;
                }
            }
            return null;
        }
    }
}