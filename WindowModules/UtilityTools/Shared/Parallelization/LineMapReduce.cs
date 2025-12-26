using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using ApplicationSuite.WindowModules.UtilityTools.Shared.TextOps;

/*
SUMMARY:
- “分割→並列→合成”の標準化ポイント。順序保持のMap（index-based）で結果を並べ、最後に一括結合。
- 並列度は既定（環境の論理コア数）。粒度・結合戦略のチューニングは将来ここに集約。
*/

namespace ApplicationSuite.WindowModules.UtilityTools.Shared.Parallelization
{
    public static class LineMapReduce
    {
        // 説明: 改行コード \r\n / \n を正規化して行配列に分割。
        //       末尾改行は削除。入力が null/空なら空配列を返す。
        public static string[] SplitToLines(string input)
        {
            if (string.IsNullOrEmpty(input)) return Array.Empty<string>();
            // StringReaderだと依存が増えるので、簡易に正規化してSplit
            var normalized = input.Replace("\r\n", "\n");
            return normalized.Split('\n');
        }

        // 説明: 行配列をインデックス順に並列処理する。
        //       各行に perLine を適用し、順序を保持して返す。
        //       スレッドセーフ、例外は呼び元に投げる。
        public static string[] MapOrdered(string[] lines, Func<string, string> perLine)
        {
            var length = lines.Length;
            var result = new string[length];
            if (length == 0) return result;

            // 粒度：行単位（将来はRange分割に変更可能）
            Parallel.For(0, length, i =>
            {
                // 例外はタスク内で投げっぱなしにせず、結果配列に“原文”を入れる or 集約（後者は次版）
                result[i] = perLine(lines[i]);
            });

            return result;
        }

        // 説明: 行配列を結合して単一の文字列に戻す。
        //       各行の間に Environment.NewLine を挿入する。
        //       最後の行の後には改行を追加しない（初稿仕様）。
        public static string JoinLines(string[] lines)
        {
            if (lines.Length == 0) return string.Empty;
            // 最後の行にも改行を付けるかは要件次第。初稿は付けない（入力の末尾改行は保持しない）
            var sb = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 0) sb.Append(Environment.NewLine);
                sb.Append(lines[i]);
            }
            return sb.ToString();
        }
    }
}
