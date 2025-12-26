using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Record.StaticFunctions
{
    public static class PathOps
    {
        /// <summary>
        /// root からの相対パスを返す。失敗したら path をそのまま返す（例外を投げない）。
        /// </summary>
        public static string Relative(string root, string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(path))
                    return path ?? string.Empty;
                return Path.GetRelativePath(root, path);
            }
            catch
            {
                return path ?? string.Empty;
            }
        }

        /// <summary>
        /// ファイル名だけを返す（Path.GetFileName の薄いラッパ）。
        /// </summary>
        public static string FileName(string? path) => Path.GetFileName(path);
    }
}
