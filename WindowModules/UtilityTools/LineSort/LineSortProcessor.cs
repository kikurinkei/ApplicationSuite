using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;
using ApplicationSuite.WindowModules.UtilityTools.Shared.Parallelization;
using ApplicationSuite.WindowModules.UtilityTools.Shared.TextOps;

namespace ApplicationSuite.WindowModules.UtilityTools.LineSort
{
    public class LineSortProcessor
    {
        private readonly LineSortSession _session = new();

        public string Run(string operationName, string inputText, bool ascending)
        {
            if (operationName != "SortAsc" && operationName != "SortDesc") return inputText;
            return _session.Run(inputText, ascending);
        }
    }

    public class LineSortSession
    {
        // NOTE: ソートは「配列全体」を比較するので Map ではなく一括処理
        public string Run(string inputText, bool ascending)
        {
            var lines = LineMapReduce.SplitToLines(inputText); // 正規化＆分割:contentReference[oaicite:4]{index=4}
            var sorted = LineOps.SortOps.SortLines(lines, ascending);  // 共有のソート関数（Ordinalで決定的）
            return LineMapReduce.JoinLines(sorted);            // 結合（末尾改行なし）:contentReference[oaicite:5]{index=5}
        }
    }
}
