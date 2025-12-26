using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.AppGenerator.Catalog
{
    public static class BitFilter
    {
        public static Dictionary<string, UtilityMetaInfo> FilterByTypeFlag(
            Dictionary<string, UtilityMetaInfo> details,
            int targetFlag)
        {
            return details
                .Where(pair =>
                    pair.Value.UITypeFlags != 0 && // UtilityMetaInfo が構造体なら null チェック不要
                    (pair.Value.UITypeFlags & targetFlag) != 0)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }


        //// 順序保証：Dictionary は列挙順が .NET Core 以降で挿入順ですが、
        //// 明示するためにインデックス化して保持
        //return details
        //    .Select((pair, index) => new { pair.Key, pair.Value, index })
        //    .Where(x =>
        //        x.Value.TypeFlags != 0 && // 修正: UtilityMetaInfo は構造体で null チェックは不要
        //        (x.Value.TypeFlags & targetFlag) != 0)
        //    .OrderBy(x => x.index)
        //    .Select(x => x.Key)
        //    .ToList();
    }
}
