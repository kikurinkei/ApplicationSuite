using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Download
{
    /// <summary>
    /// LZH のダウンロード URI を組み立てる純粋ヘルパ（副作用なし）。
    /// 旧実装の規則：
    ///  - RP: https://www1.mbrace.or.jp/od2/B/{yyyyMM}/b{yyMMdd}.lzh
    ///  - RR: https://www1.mbrace.or.jp/od2/K/{yyyyMM}/k{yyMMdd}.lzh
    /// </summary>
    public static class UriComposer
    {
        public sealed record UriItem(DateOnly Date, string UriString, string FileName);

        public static IReadOnlyList<UriItem> BuildRpUris(IReadOnlyList<DateOnly> dates)
            => BuildUris(dates, basePath: "https://www1.mbrace.or.jp/od2/B/", prefix: "b");

        public static IReadOnlyList<UriItem> BuildRrUris(IReadOnlyList<DateOnly> dates)
            => BuildUris(dates, basePath: "https://www1.mbrace.or.jp/od2/K/", prefix: "k");

        private static IReadOnlyList<UriItem> BuildUris(IReadOnlyList<DateOnly> dates, string basePath, string prefix)
        {
            var list = new List<UriItem>(dates.Count);
            foreach (var d in dates)
            {
                var yyyyMM = d.ToString("yyyyMM");
                var yyMMdd = d.ToString("yyMMdd");
                var file = $"{prefix}{yyMMdd}.lzh";
                var uri = $"{basePath}{yyyyMM}/{file}";
                list.Add(new UriItem(d, uri, file));
            }
            return list;
        }
    }
}
