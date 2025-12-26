using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.AppGenerator.Configuration
{
    public static class ConfigResolver
    {
        /// <summary>
        /// Suite構成の中から "Category" プロパティが "Root" のものを探し、
        /// その ElementId を返す。
        /// Suiteファイル群には、1つだけ "Category": "Root" が存在する想定。
        /// </summary>
        public static string? GetSuiteRootId()
        {
            var suiteElements = ConfigStore.GetSuiteElements();

            foreach (KeyValuePair<string, ElementDetail> kv in suiteElements)
            {
                ElementDetail element = kv.Value;
                if (element.Category != null && element.Category == "Root")
                {
                    // 該当の構成が見つかったら、そのIDを返す
                    return kv.Key;
                }
            }
            // 該当なし
            return null;
        }

        /// <summary>
        /// Suite構成の指定IDから、最初の子要素ID（Shell構成の起点）を取得する。
        /// </summary>
        /// <param name="suiteId">Suite構成のエレメントID</param>
        /// <returns>Shell構成の先頭ID。存在しない場合は null。</returns>
        public static string? GetShellEntryId(string suiteId)
        {
            ElementDetail? suite = ConfigStore.GetSuiteElement(suiteId);
            if (suite == null || suite.ChildElementIds == null || suite.ChildElementIds.Count == 0)
            {
                // 子要素がない場合は取得できない
                return null;
            }


            return suite.ChildElementIds[0];
        }

        // ManualRootDir
        public static string? GetManualRootDir(string suiteId)
        {
            ElementDetail? suite = ConfigStore.GetSuiteElement(suiteId);
            return suite?.ManualRootDir;
        }


    }
}
