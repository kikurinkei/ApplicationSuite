using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Record.StaticFunctions
{
    public static class ParseFileDate
    {
        /// <summary>
        /// ファイル名から日付を抽出し、yyyyMMdd形式の文字列を返す
        /// 例: "K250903.TXT" → "20250903"
        /// </summary>
        public static string FromFileName(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            // 先頭1文字は種別 (K/Bなど)、次の6桁が yymmdd
            if (fileName.Length < 7)
                throw new ArgumentException($"ファイル名が不正です: {fileName}");

            string yymmdd = fileName.Substring(1, 6);

            // yy → 2000年代として扱う（必要なら世紀判定を拡張）
            int year = 2000 + int.Parse(yymmdd.Substring(0, 2));
            int month = int.Parse(yymmdd.Substring(2, 2));
            int day = int.Parse(yymmdd.Substring(4, 2));

            var date = new DateTime(year, month, day);

            return date.ToString("yyyyMMdd");
        }
    }
}
