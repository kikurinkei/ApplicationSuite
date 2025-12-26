using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using ApplicationSuite.WindowModules.UtilityTools.Shared.Parallelization;
using ApplicationSuite.WindowModules.UtilityTools.Shared.TextOps;

namespace ApplicationSuite.WindowModules.UtilityTools.LineReplaceLiteral
{
    public class LineReplaceLiteralProcessor
    {
        private readonly LineReplaceLiteralSession _session = new();

        public string Run(string operationName, string inputText,
                          string find, string replacement, bool caseSensitive)
        {
            if (operationName != "Replace") return inputText;
            if (string.IsNullOrEmpty(find)) return inputText; // 空Findは無操作

            return _session.Run(inputText, line => LineOps.ReplacePlain(line, find, replacement, caseSensitive));
        }
    }

    public class LineReplaceLiteralSession
    {
        public string Run(string inputText, Func<string, string> perLine)
        {
            var lines = LineMapReduce.SplitToLines(inputText);
            var transformed = LineMapReduce.MapOrdered(lines, perLine);
            return LineMapReduce.JoinLines(transformed);
        }
    }
}
