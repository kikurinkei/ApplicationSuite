using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using ApplicationSuite.WindowModules.UtilityTools.Shared.Parallelization;
using ApplicationSuite.WindowModules.UtilityTools.Shared.TextOps;

namespace ApplicationSuite.WindowModules.UtilityTools.SpaceCompact
{
    public class SpaceCompactProcessor
    {
        private readonly SpaceCompactSession _session = new();

        public string Run(string operationName, string inputText)
        {
            // 操作名に応じて 1 行関数を切替（浅い switch）
            switch (operationName)
            {
                case "CollapseSpaces":
                    return _session.Run(inputText, LineOps.CollapseSpaces);
                case "CollapseMixedWhitespace":
                    return _session.Run(inputText, LineOps.CollapseMixedWhitespace);
                case "CollapseTabs":
                    return _session.Run(inputText, LineOps.CollapseTabs);
                default:
                    return inputText;
            }
        }
    }

    public class SpaceCompactSession
    {
        public string Run(string inputText, Func<string, string> perLine)
        {
            // 分割→並列（順序保持）→結合：既存の共通基盤を流用
            var lines = LineMapReduce.SplitToLines(inputText);
            var transformed = LineMapReduce.MapOrdered(lines, perLine);
            return LineMapReduce.JoinLines(transformed);
        }
    }
}
