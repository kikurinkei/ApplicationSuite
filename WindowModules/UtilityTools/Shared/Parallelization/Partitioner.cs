using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
SUMMARY:
- 予備の等分/動的分割ユーティリティ（初稿では未使用でもOK）。
- 将来、長行の偏り対策やチャンク粒度調整で採用予定。
*/

namespace ApplicationSuite.WindowModules.UtilityTools.Shared.Parallelization
{
    public static class Partitioner
    {
        // 説明: 総要素数 total を parts 等分し、開始/終了インデックス範囲を返す。
        //       分割数が total を超える場合は total に丸める。
        //       余りは前方の範囲に +1 ずつ割り振る。
        public static (int start, int end)[] EvenRanges(int total, int parts)
        {
            if (total <= 0 || parts <= 0) return Array.Empty<(int, int)>();
            parts = Math.Min(parts, total);
            var ranges = new (int, int)[parts];

            int baseSize = total / parts;
            int remainder = total % parts;
            int index = 0;
            for (int p = 0; p < parts; p++)
            {
                int size = baseSize + (p < remainder ? 1 : 0);
                ranges[p] = (index, index + size);
                index += size;
            }
            return ranges;
        }
    }
}
