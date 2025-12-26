using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

/*
SUMMARY:
- 1行→1行の純粋関数群（副作用なし）。並列安全でテスト・最適化しやすい“核”。
*/

namespace ApplicationSuite.WindowModules.UtilityTools.Shared.TextOps
{
    public static class LineOps
    {
        // 説明: 行頭に prefix を追加する。
        //       prefix が空文字の場合は入力をそのまま返す。
        public static string AddPrefixToLine(string line, string prefix)
        {
            // Prefixが空なら無変化を返す（初稿の仕様）
            if (string.IsNullOrEmpty(prefix)) return line;
            return prefix + line;
        }
        // 説明: 行頭が prefix と一致する場合のみ除去する。
        //       大小・全半角は区別。prefix が空文字の場合は入力そのまま。
        public static string RemovePrefixFromLine(string line, string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return line;
            // 先頭一致のみ除去（部分一致や空白無視はしない：別機能で扱う前提）
            return line.StartsWith(prefix) ? line.Substring(prefix.Length) : line;
        }
        // 説明: 行頭が prefix の場合は除去、それ以外は prefix を追加。
        //       prefix が空文字の場合は入力そのまま。
        public static string TogglePrefixLine(string line, string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return line;
            return line.StartsWith(prefix) ? line.Substring(prefix.Length) : prefix + line;
        }

        // --- Suffix 系（Prefix と対称）---
        // 説明: 行末に suffix を追加。suffix が空文字の場合は入力をそのまま返す。大小・全半角は区別。
        public static string AddSuffixToLine(string line, string suffix)
        {
            if (line is null) return string.Empty;
            if (string.IsNullOrEmpty(suffix)) return line; // 空なら無操作
            return line + suffix;
        }
        // 説明: 行末が suffix と完全一致する場合のみ除去。suffix が空文字の場合は入力そのまま。大小・全半角は区別。
        public static string RemoveSuffixFromLine(string line, string suffix)
        {
            if (line is null) return string.Empty;
            if (string.IsNullOrEmpty(suffix)) return line; // 空なら無操作
            return line.EndsWith(suffix, StringComparison.Ordinal)
                ? line.Substring(0, line.Length - suffix.Length)
                : line;
        }
        // 説明: 行末が suffix の場合は除去、それ以外は suffix を追加。suffix が空文字の場合は入力そのまま。大小・全半角は区別。
        public static string ToggleSuffixLine(string line, string suffix)
        {
            if (line is null) return string.Empty;
            if (string.IsNullOrEmpty(suffix)) return line; // 空なら無操作
            return line.EndsWith(suffix, StringComparison.Ordinal)
                ? line.Substring(0, line.Length - suffix.Length)
                : line + suffix;
        }

        // --- Trim 系 ---
        // 列挙型（どちらをTrimするか）
        public enum TrimSide { Both, Left, Right }
        public static string TrimLine(string line, TrimSide side)
        {
            if (line is null) return string.Empty;
            return side switch
            {
                TrimSide.Left => line.TrimStart(),
                TrimSide.Right => line.TrimEnd(),
                _ => line.Trim()
            };
        }
        // --- Space Convert 系 ---
        // NOTE: Tabs→Spaces は「位置依存のタブストップ」ではなく、1タブ=tabWidth個の半角スペースに単純置換。
        //       表組みなど“見た目の桁位置”を厳密に合わせたい場合は、将来の拡張（列位置に応じた展開）を検討。
        public static string TabsToSpaces(string line, int tabWidth)
        {
            if (line is null) return string.Empty;
            if (tabWidth < 1) tabWidth = 1;

            // '\t' を固定長のスペース列に置換（単純モデル）
            var spaces = new string(' ', tabWidth);
            return line.Replace("\t", spaces);
        }

        // 連続する「tabWidth 個の半角スペース」を '\t' に圧縮。
        // 長いスペース列は複数回の置換で段階的に圧縮される（例：8個→2タブ）。
        public static string SpacesToTabs(string line, int tabWidth)
        {
            if (line is null) return string.Empty;
            if (tabWidth < 1) tabWidth = 1;

            var spaces = new string(' ', tabWidth);
            // 置換を複数回適用して、まだ置換できる塊がある限り圧縮する
            string prev;
            string cur = line;
            do
            {
                prev = cur;
                cur = cur.Replace(spaces, "\t");
            } while (!ReferenceEquals(prev, cur) && prev != cur);

            return cur;
        }

        // 全角スペース（U+3000）→ 半角スペース
        public static string ZenkakuSpaceToHankaku(string line)
        {
            if (line is null) return string.Empty;
            return line.Replace('　', ' '); // U+3000
        }

        // 半角スペース → 全角スペース（U+3000）
        public static string HankakuSpaceToZenkaku(string line)
        {
            if (line is null) return string.Empty;
            return line.Replace(' ', '　'); // U+3000
        }


        // --- Space Compact 系 ---
        // 1) 半角スペースの連続を 1 個に圧縮（"  " 以上 → " "）
        public static string CollapseSpaces(string line)
        {
            if (line is null) return string.Empty;
            // 2 個以上の連続半角スペースを 1 個に
            return Regex.Replace(line, " {2,}", " ");
        }

        // 2) 「半角スペース / タブ / 全角スペース」の連続を 1 個の半角スペースに正規化
        //    例: " \t　 例\t\t文" → " 例 文"
        public static string CollapseMixedWhitespace(string line)
        {
            if (line is null) return string.Empty;
            // [space | tab | zenkaku-space] の 2 個以上の連続を " " に
            return Regex.Replace(line, "[ \\t　]{2,}", " ");
        }

        // 3) タブの連続を 1 個に圧縮（"\t\t+" → "\t"）
        public static string CollapseTabs(string line)
        {
            if (line is null) return string.Empty;
            return Regex.Replace(line, "\\t{2,}", "\t");
        }


        // --- Replace（Regex）---
        // pattern: 正規表現（CultureInvariant 前提）、opts: IgnoreCase/Multiline 等
        // replacement: .NET Regex 準拠（$1 などのグループ置換可）
        public static string ReplaceRegex(string line, string pattern, string replacement, RegexOptions opts)
        {
            if (line is null) return string.Empty;
            if (string.IsNullOrEmpty(pattern)) return line;
            return Regex.Replace(line, pattern, replacement ?? string.Empty, opts | RegexOptions.CultureInvariant);
        }

        // --- Replace（Literal/文字そのまま）---
        // CaseSensitive=false の場合も、文化非依存で大文字小文字を無視するため Regex を利用。
        // ・find を Regex.Escape してリテラルに固定化
        // ・IgnoreCase は Ordinal（CultureInvariant）で適用
        public static string ReplacePlain(string line, string find, string replacement, bool caseSensitive)
        {
            if (line is null) return string.Empty;
            if (string.IsNullOrEmpty(find)) return line;

            var pattern = Regex.Escape(find);
            var opts = RegexOptions.CultureInvariant;
            if (!caseSensitive) opts |= RegexOptions.IgnoreCase;
            return Regex.Replace(line, pattern, replacement ?? string.Empty, opts);
        }


        // --- LineNumbering 系 ---
        // 桁埋め + 接続子連結の Prefix を作成し、line の先頭に付与
        public static string AddLineNumber(string line, int number, int padWidth, string connector)
        {
            if (line is null) return string.Empty;
            if (padWidth < 0) padWidth = 0;

            // number を桁埋め："D{padWidth}" 形式。padWidth=0 は ToString() と同等。
            string num = padWidth > 0 ? number.ToString("D" + padWidth) : number.ToString();

            // Connector が null の場合は空文字として扱う（自由入力仕様）
            string conn = connector ?? string.Empty;

            // "01. " のような前置を生成して結合
            return num + conn + line;
        }

        // 行頭に「1回だけ」数字 + connector があれば除去（数字桁は任意）
        // 例: connector=". " → ^\d+\<dot><space>
        public static string RemoveLineNumber(string line, string connector)
        {
            if (line is null) return string.Empty;

            string conn = connector ?? string.Empty;
            // Connector をリテラル扱いにするためエスケープ
            string connEscaped = Regex.Escape(conn);

            // ^\d+<conn> の最左一致を 1 回だけ削除
            return Regex.Replace(line, @"^\d+" + connEscaped, string.Empty, RegexOptions.CultureInvariant);
        }

        public static class SortOps
        {
            // 行配列を Ordinal 比較でソート（昇順/降順）
            public static string[] SortLines(string[] lines, bool ascending)
            {
                if (lines is null) return Array.Empty<string>();
                var copy = (string[])lines.Clone(); // 破壊的変更を避ける
                Array.Sort(copy, StringComparer.Ordinal);
                if (!ascending) Array.Reverse(copy);
                return copy;
            }
        }

        public static class CountOps
        {
            // 半角=1 / 全角=2 の簡易カウント（Unicodeレンジに基づくヒューリスティック）
            // - 全角扱い(=2): Hiragana, Katakana, CJK統合漢字, Hangul, CJK系記号, 全角形状(FF01–FF60, FFE0–FFE6) 等
            // - 半角扱い(=1): ASCII, 半角ｶﾀｶﾅ(FF61–FF9F), 一般的な記号, サロゲート外の補助記号
            // - サロゲート(サロゲートペア)は Rune 単位で 2 or 1 を判定（基本は1。CJK拡張A等は2）
            public static int CountZenkakuStyle(string text)
            {
                if (string.IsNullOrEmpty(text)) return 0;

                int total = 0;
                var e = text.EnumerateRunes(); // .NET: System.Text.Rune を用いてサロゲートも1単位で扱う
                foreach (var r in e)
                {
                    total += IsFullWidthLike(r) ? 2 : 1;
                }
                return total;
            }

            // 代表的レンジによる簡易判定（必要に応じて拡張可）
            private static bool IsFullWidthLike(Rune r)
            {
                int u = r.Value;

                // CJK Symbols & Punctuation, Hiragana, Katakana
                if ((u >= 0x3000 && u <= 0x303F) || (u >= 0x3040 && u <= 0x309F) || (u >= 0x30A0 && u <= 0x30FF))
                    return true;

                // CJK Unified Ideographs (基本面) + 拡張A
                if ((u >= 0x3400 && u <= 0x4DBF) || (u >= 0x4E00 && u <= 0x9FFF))
                    return true;

                // Hangul Syllables
                if (u >= 0xAC00 && u <= 0xD7A3)
                    return true;

                // Fullwidth Forms (一部を全角記号として扱う)
                if ((u >= 0xFF01 && u <= 0xFF60) || (u >= 0xFFE0 && u <= 0xFFE6))
                    return true;

                // それ以外は半角相当（Halfwidth Katakana FF61–FF9F は半角=1）
                return false;
            }
        }
    }
}

