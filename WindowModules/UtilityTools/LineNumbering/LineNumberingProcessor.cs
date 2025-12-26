using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using ApplicationSuite.WindowModules.UtilityTools.Shared.Parallelization;
using ApplicationSuite.WindowModules.UtilityTools.Shared.TextOps;

namespace ApplicationSuite.WindowModules.UtilityTools.LineNumbering
{
    public class LineNumberingProcessor
    {
        private readonly LineNumberingSession _session = new();

        // NOTE: operationName は将来拡張のために受け取る（統一シグネチャ）
        public string Run(string operationName, string inputText, int startNumber, int padWidth, string connector)
        {
            switch (operationName)
            {
                case "AddNumbers":
                    // 連番は行インデックス依存なので **順次処理**（非並列）
                    return _session.RunAddSequential(inputText, startNumber, padWidth, connector);

                case "RemoveNumbers":
                    // 除去は per-line で独立しているため Map で OK
                    return _session.RunPerLine(inputText, line => LineOps.RemoveLineNumber(line, connector));

                default:
                    return inputText;
            }
        }
    }

    public class LineNumberingSession
    {
        // 付与：Split → 順次番号付与 → Join
        public string RunAddSequential(string inputText, int startNumber, int padWidth, string connector)
        {
            var lines = LineMapReduce.SplitToLines(inputText); // 分割（順序保持）:contentReference[oaicite:2]{index=2}
            var output = new string[lines.Length];

            // 行ごとに index ベースで番号を生成
            for (int i = 0; i < lines.Length; i++)
            {
                int number = startNumber + i;
                // Prefixを生成して結合（LineOps で桁埋めと接続子連結を担当）
                output[i] = LineOps.AddLineNumber(lines[i], number, padWidth, connector);
            }

            return LineMapReduce.JoinLines(output); // 結合（末尾改行なし）:contentReference[oaicite:3]{index=3}
        }

        // 除去：Split → Map（独立処理）→ Join
        public string RunPerLine(string inputText, Func<string, string> perLine)
        {
            var lines = LineMapReduce.SplitToLines(inputText);
            var transformed = LineMapReduce.MapOrdered(lines, perLine); // ここは並列/順序保持でOK:contentReference[oaicite:4]{index=4}
            return LineMapReduce.JoinLines(transformed);
        }
    }
}
