using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Record.StaticFunctions.Versions
{
    /// <summary>
    /// RaceHeader（rR 含む固定ブロック）のミニマム抽出。
    /// 仕様（1始まり表記）:
    ///  - rR 行の「1〜4文字目」を Trim → int として RaceNo（1..12）
    ///  - rR 行を 1 として、4〜9 行目が 1〜6 人目
    ///  - 各レーサー行の「9〜12文字目」（4桁・半角）を登録番号 RegNo として int 化
    ///  - 探索せず、固定位置のみを逐次取得（i++ 方針）
    /// </summary>
    public static class RaceHeaderPick
    {
        public static void ReadRaceNoAndRegs(
            string[] lines, int raceStartIndex,
            out int raceNo,
            out int regNo1, out int regNo2, out int regNo3,
            out int regNo4, out int regNo5, out int regNo6)
        {
            // rR 行（1〜4文字目）
            var rLine = GetLine(lines, raceStartIndex + 0);
            var rHead = SubstringByChar(rLine, 0, 4);     // 0始まり: [0..4)
            raceNo = int.Parse(rHead.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture);

            // rR を1として 4..9 行目が 1..6 人目
            regNo1 = ParseRegNo(lines, raceStartIndex + 3); // 4行目
            regNo2 = ParseRegNo(lines, raceStartIndex + 4); // 5行目
            regNo3 = ParseRegNo(lines, raceStartIndex + 5); // 6行目
            regNo4 = ParseRegNo(lines, raceStartIndex + 6); // 7行目
            regNo5 = ParseRegNo(lines, raceStartIndex + 7); // 8行目
            regNo6 = ParseRegNo(lines, raceStartIndex + 8); // 9行目
        }

        // --- helpers（探索・冗長ガードは置かない。固定前提の最小実装） ---

        private static int ParseRegNo(string[] lines, int index)
        {
            var s = GetLine(lines, index);
            var slice = SubstringByChar(s, 8, 4); // 9〜12文字目（1始まり）→ 0始まり index=8, length=4
            return int.Parse(slice.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        private static string GetLine(string[] lines, int index)
        {
            // Validation通過前提。範囲外は例外（隠し救済は入れない）。
            return lines[index] ?? string.Empty;
        }

        /// <summary>
        /// 文字単位の部分文字列（0始まり index, length）。
        /// サロゲート等は考慮しない（ヘッダの想定内で十分）。
        /// </summary>
        private static string SubstringByChar(string s, int startIndexZeroBased, int length)
        {
            return s.Substring(
                startIndexZeroBased,
                Math.Min(length, Math.Max(0, s.Length - startIndexZeroBased))
            );
        }
    }
}