using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Record.StaticFunctions.Versions
{
    /// <summary>
    /// 払戻金ブロックから配当を拾う（初稿は単勝のみ）。
    /// 将来的に複勝・連単などを追加できるように、ゆるめの設計。
    /// </summary>
    public static class Kv1OddsPick
    {
        /// <summary>
        /// 単勝配当を拾う。
        /// 仕様（暫定）:
        ///   - oddsStartIndex 行から相対的に単勝の金額がある行を読む
        ///   - 固定位置から substring → trim → decimal 化
        /// </summary>
        public static decimal ReadWinOdds(string[] lines, int oddsStartIndex)
        {
            // ★ 暫定：とりあえず oddsStartIndex 行そのものから拾う
            // 実際には「払戻金ブロックの何行目に単勝があるか」を仕様に合わせて調整する
            var line = lines[oddsStartIndex] ?? string.Empty;

            // 例: 先頭〜10文字目に金額があると仮定（仕様に応じて修正）
            var slice = SubstringByChar(line, 0, 10).Trim();

            if (decimal.TryParse(slice, NumberStyles.Number, CultureInfo.InvariantCulture, out var odds))
                return odds;

            // 解析失敗時は例外（暫定）
            throw new FormatException($"単勝配当の解析に失敗しました（line='{line}'）。");
        }

        // --- helper ---
        private static string SubstringByChar(string s, int startIndexZeroBased, int length)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            if (startIndexZeroBased < 0) startIndexZeroBased = 0;
            if (startIndexZeroBased >= s.Length) return string.Empty;
            if (length < 0) length = 0;
            if (startIndexZeroBased + length > s.Length)
                length = s.Length - startIndexZeroBased;
            return s.Substring(startIndexZeroBased, length);
        }
    }
}
