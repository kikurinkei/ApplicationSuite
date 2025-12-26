using ApplicationSuite.AppGenerator.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.AppGenerator.Catalog
{
    public static class BitDictionaryBuilder
    {
        /// <summary>
        /// 指定したShell IDから、配下Utility IDとTypeFlagsを辞書化して返します。
        /// </summary>
        /// <param name="shellId">Shell要素のID</param>
        /// <returns>UtilityIDをキー、TypeFlags(int)を値とする辞書</returns>
        public static Dictionary<string, UtilityMetaInfo> BitDictionary(string shellId)
        {
            var result = new Dictionary<string, UtilityMetaInfo>();

            // Shell要素の取得
            var shellElement = ConfigStore.GetShellElement(shellId);
            if (shellElement == null || shellElement.ChildElementIds == null)
            {
                return result; // 空の辞書を返す
            }

            foreach (var utilityId in shellElement.ChildElementIds)
            {
                var utilityElement = ConfigStore.GetUtilityElement(utilityId);

                // Utility要素が存在し、TypeFlagsが設定されている場合のみ登録
                if (utilityElement?.ElementId != null &&
                    utilityElement?.ElementName != null &&
                    utilityElement?.Description != null &&
                    utilityElement?.Usage != null &&
                    utilityElement?.IconPath != null &&
                    utilityElement?.TypeFlags != null &&
                    utilityElement?.ViewModelPath != null &&
                    utilityElement?.ControlPath != null)
                {
                    result[utilityId] = new UtilityMetaInfo
                    {
                        UIElementId = utilityElement.ElementId,
                        UIElementName = utilityElement.ElementName,
                        UIDescription = utilityElement.Description,
                        UIUsage = utilityElement.Usage,
                        UIIconPath = utilityElement.IconPath,
                        UITypeFlags = (int)utilityElement.TypeFlags,
                        UIViewModelPath = utilityElement.ViewModelPath,
                        UIControlPath = utilityElement.ControlPath,
                    };
                }
            }

            return result;
        }

        //public struct UtilityMetaInfo
        //{
        //    public string UIElementId { get; set; }
        //    public string UIElementName { get; set; }
        //    public string UIDescription { get; set; }
        //    public string UIUsage { get; set; }
        //    public string UIIconPath { get; set; }
        //    public int UITypeFlags { get; set; }
        //    public string UIViewModelPath { get; set; }
        //    public string UIControlPath { get; set; }
        //}
    }
}
