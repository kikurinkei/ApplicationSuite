using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Record.StaticFunctions.Versions
{
    /// <summary>
    /// R見出し行から RaceNo を抜くだけの緩いv1。
    /// 例: "   1R   一般..." -> 1
    /// </summary>
    public static class Kv1RaceHeaderReader
    {
        static readonly Regex RxRace = new(@"\b(\d{1,2})R\b", RegexOptions.Compiled);

        public static bool TryReadRaceNo(string line, out int raceNo)
        {
            var m = RxRace.Match(line ?? string.Empty);
            if (m.Success && int.TryParse(m.Groups[1].Value, out raceNo))
                return raceNo is >= 1 and <= 12;
            raceNo = 0;
            return false;
        }
    }
}
