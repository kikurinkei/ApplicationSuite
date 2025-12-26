using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using ApplicationSuite.WindowModules.UtilityTools.Shared.Parallelization;
using ApplicationSuite.WindowModules.UtilityTools.Shared.TextOps;

namespace ApplicationSuite.WindowModules.UtilityTools.LineTrim
{
    public class LineTrimProcessor
    {
        private readonly LineTrimSession _session = new();

        public string Run(string inputText, string trimMode)
        {
            // trimModeに応じて perLine処理を切替
            return trimMode switch
            {
                "Left" => _session.Run(inputText, line => LineOps.TrimLine(line, LineOps.TrimSide.Left)),
                "Right" => _session.Run(inputText, line => LineOps.TrimLine(line, LineOps.TrimSide.Right)),
                _ => _session.Run(inputText, line => LineOps.TrimLine(line, LineOps.TrimSide.Both))
            };
        }
    }

    public class LineTrimSession
    {
        public string Run(string inputText, Func<string, string> perLine)
        {
            var lines = LineMapReduce.SplitToLines(inputText); // 行分割（改行正規化）  
            var transformed = LineMapReduce.MapOrdered(lines, perLine); // 順序保持並列  
            return LineMapReduce.JoinLines(transformed); // 結合
        }
    }
}
