using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Record.StaticFunctions
{
    /// <summary>
    /// ファイル名（KYYMMDD.TXT）から RDATE を取得する純粋関数。
    /// 例: K250903.TXT -> 2025-09-03
    /// </summary>
    public static class FileNameDateOps
    {
        private static readonly Regex RxK = new(@"^[Kk](\d{6})\.txt$", RegexOptions.Compiled);

        /// <summary>
        /// ファイル名からレース日付（RDATE）を取得。
        /// 不一致の場合は false を返す（呼び出し側で line=0 として扱う前提）。
        /// </summary>
        public static bool TryParseKFileDate(string filePath, out DateOnly rdate)
        {
            rdate = default;

            var name = Path.GetFileName(filePath);
            var m = RxK.Match(name);
            if (!m.Success) return false;

            var yymmdd = m.Groups[1].Value; // 例: "250903"
            var yy = int.Parse(yymmdd[..2]);
            var mm = int.Parse(yymmdd.Substring(2, 2));
            var dd = int.Parse(yymmdd.Substring(4, 2));

            rdate = new DateOnly(2000 + yy, mm, dd);
            return true;
        }
    }
}
