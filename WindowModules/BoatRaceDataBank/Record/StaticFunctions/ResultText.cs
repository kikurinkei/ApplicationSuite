using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Record.IO;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Record.Models;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Record.StaticFunctions;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Record.StaticFunctions
{
    public static class ResultText
    {//
        /// <summary>
        /// Record結果を表示用テキストにする。
        /// 行フォーマット: "{tag} {FileName} | {Ok/Ng} | {Note} | {dest or -} | {ms}ms"
        /// </summary>
        public static string FromRecord(string? tag, IReadOnlyList<RecordResult> results, string root)
        {
            if (results == null || results.Count == 0) return string.Empty;

            string t = string.IsNullOrEmpty(tag) ? string.Empty : tag + " ";
            var sb = new StringBuilder(capacity: results.Count * 48);

            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                var name = PathOps.FileName(r.FilePath);
                var dest = string.IsNullOrEmpty(r.DestinationPath) ? "-" : PathOps.Relative(root, r.DestinationPath!);
                var note = r.Note ?? string.Empty;

                sb.Append(t)
                  .Append(name).Append(" | ")
                  .Append(r.Result).Append(" | ")
                  .Append(note).Append(" | ")
                  .Append(dest).Append(" | ")
                  .Append(r.ElapsedMs).Append("ms");

                if (i < results.Count - 1) sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        /// <summary>
        /// ファイル一覧を相対パスで1本のテキストにまとめる。
        /// </summary>
        public static string FromFileList(string root, IEnumerable<string> files)
        {
            if (files == null) return string.Empty;
            return string.Join(Environment.NewLine, files.Select(p => PathOps.Relative(root, p)));
        }


        /// <summary>
        /// Move結果を表示用テキストにする。
        /// 行: "{tag} {FileName} | {Ok/Ng} | {Note} | {relativeDest or -} | {ms}ms"
        /// </summary>
        public static string FromMove(string? tag, IReadOnlyList<MoveResult> results, string destRoot)
        {
            if (results == null || results.Count == 0) return string.Empty;

            string t = string.IsNullOrEmpty(tag) ? string.Empty : tag + " ";
            var sb = new StringBuilder(capacity: results.Count * 48);

            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                var name = PathOps.FileName(r.FilePath);
                var dest = string.IsNullOrEmpty(r.DestinationPath) ? "-" : PathOps.Relative(destRoot, r.DestinationPath!);
                var note = r.Note ?? string.Empty;

                sb.Append(t)
                  .Append(name).Append(" | ")
                  .Append(r.Result).Append(" | ")
                  .Append(note).Append(" | ")
                  .Append(dest).Append(" | ")
                  .Append(r.ElapsedMs).Append("ms");

                if (i < results.Count - 1) sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
