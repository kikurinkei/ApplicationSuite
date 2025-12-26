using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.WindowModules.UtilityTools.Shared.TextOps;

namespace ApplicationSuite.WindowModules.UtilityTools.CharCount
{
    public class CharCountProcessor
    {
        public string Run(string operationName, string inputText)
        {
            if (operationName != "Count") return inputText;

            // 半角=1 / 全角=2 の“バイト風長さ”
            int count = LineOps.CountOps.CountZenkakuStyle(inputText);
            return count.ToString();
        }
    }
}
