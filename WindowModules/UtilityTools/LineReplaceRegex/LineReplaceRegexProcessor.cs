using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ApplicationSuite.WindowModules.UtilityTools.Shared.Parallelization;
using ApplicationSuite.WindowModules.UtilityTools.Shared.TextOps;

namespace ApplicationSuite.WindowModules.UtilityTools.LineReplaceRegex
{
    public class LineReplaceRegexProcessor
    {
        private readonly LineReplaceRegexSession _session = new();

        // Run: operationName を先頭に（統一）
        public string Run(string operationName, string inputText,
                          string pattern, string replacement, bool ignoreCase, bool useMultiline)
        {
            if (operationName != "Replace") return inputText;
            if (string.IsNullOrEmpty(pattern)) return inputText; // 空パターンは無操作

            var opts = RegexOptions.CultureInvariant;
            if (ignoreCase) opts |= RegexOptions.IgnoreCase;
            if (useMultiline) opts |= RegexOptions.Multiline;

            return _session.Run(inputText, line => LineOps.ReplaceRegex(line, pattern, replacement, opts));
        }
    }

    public class LineReplaceRegexSession
    {
        public string Run(string inputText, Func<string, string> perLine)
        {
            var lines = LineMapReduce.SplitToLines(inputText);
            var transformed = LineMapReduce.MapOrdered(lines, perLine);
            return LineMapReduce.JoinLines(transformed);
        }
    }
}
