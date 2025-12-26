using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using ApplicationSuite.WindowModules.UtilityTools.Shared.Parallelization;
using ApplicationSuite.WindowModules.UtilityTools.Shared.TextOps;

namespace ApplicationSuite.WindowModules.UtilityTools.SpaceConvert
{
    public class SpaceConvertProcessor
    {
        private readonly SpaceConvertSession _session = new();

        public string Run(string operationName, string inputText, int? tabWidth)
        {
            switch (operationName)
            {
                case "TabsToSpaces":
                    // 1タブ = tabWidth 個の半角スペースに単純置換
                    return _session.Run(inputText, line => LineOps.TabsToSpaces(line, tabWidth ?? 4));

                case "SpacesToTabs":
                    // 連続 tabWidth 個の半角スペースを \t に圧縮（複数回適用される前提）
                    return _session.Run(inputText, line => LineOps.SpacesToTabs(line, tabWidth ?? 4));

                case "ZenkakuToHankaku":
                    // 全角スペース（U+3000）を半角スペース ' ' に
                    return _session.Run(inputText, line => LineOps.ZenkakuSpaceToHankaku(line));

                case "HankakuToZenkaku":
                    // 半角スペース ' ' を全角スペース（U+3000）に
                    return _session.Run(inputText, line => LineOps.HankakuSpaceToZenkaku(line));

                default:
                    return inputText;
            }
        }
    }

    public class SpaceConvertSession
    {
        public string Run(string inputText, Func<string, string> perLine)
        {
            var lines = LineMapReduce.SplitToLines(inputText);       // 改行正規化＋分割:contentReference[oaicite:2]{index=2}
            var transformed = LineMapReduce.MapOrdered(lines, perLine); // 順序保持並列:contentReference[oaicite:3]{index=3}
            return LineMapReduce.JoinLines(transformed);             // 結合（末尾改行なし）:contentReference[oaicite:4]{index=4}
        }
    }
}
