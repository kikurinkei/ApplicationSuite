using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Record.StaticFunctions
{
    /// <summary>
    /// Venue ヘッダの固定行取り（KV1/B で共用）。
    /// 仕様：
    ///   - 6行目：大会名（前後トリム）
    ///   - 8行目：5〜6文字目を抜き出し → トリム → 半角化 → 数値化（開催日目）
    /// 行番号は「KBGN/BBGN 行を 1行目」とする 1始まりの数え方に基づく。
    /// </summary>
    public static class VenueHeaderPick
    {
        /// <param name="lines">全行</param>
        /// <param name="headerStartIndex">KBGN/BBGN の行インデックス（0始まり）</param>
        /// <param name="title">大会名（前後トリム）</param>
        /// <param name="dayNo">開催日目（数値）。解析できない場合は例外を投げます。</param>
        public static void ReadTitleAndDay(string[] lines, int headerStartIndex, out string title, out int dayNo)
        {
            // 6行目（1始まり）= headerStart + 5
            var aLine = GetLine(lines, headerStartIndex + 5);
            title = aLine?.Trim() ?? string.Empty;

            // 8行目（1始まり）= headerStart + 7
            var bLine = GetLine(lines, headerStartIndex + 7);

            // 5〜6文字目（1始まり）= 0始まり index 4, length 2
            var slice = SubstringByChar(bLine, 4, 2); // startIndex=4 (0-based), length=2
            var half = ToHalfAsciiDigits(slice).Trim();

            if (!int.TryParse(half, NumberStyles.Integer, CultureInfo.InvariantCulture, out dayNo))
                throw new FormatException($"開催日目の数値化に失敗しました（値='{half}'）。");
        }

        // --- helpers（このファイル内に閉じる：共用前提だが過剰な外出しはしない） ---

        private static string GetLine(string[] lines, int index)
        {
            if (index < 0 || index >= lines.Length)
                throw new IndexOutOfRangeException($"行インデックスが範囲外です（index={index}）。");
            return lines[index] ?? string.Empty;
        }

        /// <summary>
        /// 文字単位の部分文字列（0始まりの index と length）。
        /// 想定入力は和文等のBMP内文字（サロゲート非考慮）。KV1/B ヘッダの想定範囲では十分。
        /// </summary>
        private static string SubstringByChar(string s, int startIndexZeroBased, int length)
        {
            if (s == null) return string.Empty;
            if (startIndexZeroBased < 0) startIndexZeroBased = 0;
            if (startIndexZeroBased >= s.Length) return string.Empty;
            if (length < 0) length = 0;
            if (startIndexZeroBased + length > s.Length)
                length = s.Length - startIndexZeroBased;
            return s.Substring(startIndexZeroBased, length);
        }

        /// <summary>
        /// 全角数字（０〜９）を半角（0〜9）へ変換。その他はそのまま。
        /// </summary>
        private static string ToHalfAsciiDigits(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            Span<char> buf = stackalloc char[input.Length];
            int j = 0;
            foreach (var ch in input)
            {
                // 全角数字のUnicodeは U+FF10 (０)〜 U+FF19 (９)
                if (ch >= '０' && ch <= '９')
                {
                    buf[j++] = (char)('0' + (ch - '０'));
                }
                else
                {
                    buf[j++] = ch;
                }
            }
            return new string(buf[..j]);
        }
    }
}