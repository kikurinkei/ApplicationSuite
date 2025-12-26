using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using ApplicationSuite.WindowModules.UtilityTools.Shared.Parallelization;
using ApplicationSuite.WindowModules.UtilityTools.Shared.TextOps;

namespace ApplicationSuite.WindowModules.UtilityTools.LineSuffix
{
    public class LineSuffixProcessor
    {
        private readonly LineSuffixSession _session = new();

        public string Run(string operationName, string inputText, string suffix)
        {
            switch (operationName)
            {
                case "AddSuffix":
                    return _session.Run(inputText, line => LineOps.AddSuffixToLine(line, suffix));
                case "RemoveSuffix":
                    return _session.Run(inputText, line => LineOps.RemoveSuffixFromLine(line, suffix));
                case "ToggleSuffix":
                    return _session.Run(inputText, line => LineOps.ToggleSuffixLine(line, suffix));
                default:
                    return inputText; // 未対応は素通し
            }
        }
    }

    public class LineSuffixSession
    {
        public string Run(string inputText, Func<string, string> perLine)
        {
            var lines = LineMapReduce.SplitToLines(inputText);
            var transformed = LineMapReduce.MapOrdered(lines, perLine);
            return LineMapReduce.JoinLines(transformed);
        }
    }
}
