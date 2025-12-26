using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.WindowModules.UtilityTools.Shared.Parallelization;
using ApplicationSuite.WindowModules.UtilityTools.Shared.TextOps;
/*
SUMMARY:
- Processorは“ミニ・オーケストラ”。VMからの1呼び出しを受け、Session（並列戦略）+ Static（核）を選ぶ。
- 初稿はすべて LinePrefixSession を使い、operationName で適用する1行関数を切替。
- 将来、条件付きPrefix/Regex併用などが増えたら、系統別Processorに分割してVMの浅いswitchを維持。
*/


namespace ApplicationSuite.WindowModules.UtilityTools.LinePrefix
{
    public class LinePrefixProcessor
    {
        private readonly LinePrefixSession _session = new();

        public string Run(string operationName, string inputText, string prefix)
        {
            // NOTE: Processor内の分岐も“浅く1段”。各case=Session.Run(...)の1行。
            switch (operationName)
            {
                case "AddPrefix":
                    return _session.Run(inputText, line => LineOps.AddPrefixToLine(line, prefix));
                case "RemovePrefix":
                    return _session.Run(inputText, line => LineOps.RemovePrefixFromLine(line, prefix));
                case "TogglePrefix":
                    return _session.Run(inputText, line => LineOps.TogglePrefixLine(line, prefix));
                default:
                    return inputText; // 未対応は素通し（初稿の簡易挙動）
            }
        }
    }

    /*
    SUMMARY (Session):
    - 分割→並列→合成を担当。並列度/粒度/結合/計測/例外集約はここで完結。VMとStaticを汚さない。
    - 今回は LineMapReduce を使って順序保持のまま高速化。
    */
    public class LinePrefixSession
    {
        public string Run(string inputText, Func<string, string> perLine)
        {
            // 分割（改行統一の寛容さは LineMapReduce に任せる）
            var lines = LineMapReduce.SplitToLines(inputText);

            // 並列Map（順序保持: indexに格納）
            var transformed = LineMapReduce.MapOrdered(lines, perLine);

            // 合成（最後に一括結合: 初稿の既定）
            return LineMapReduce.JoinLines(transformed);
        }
    }
}
