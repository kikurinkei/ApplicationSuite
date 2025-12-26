using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Validation.IO
{
    public static class PathMapper
    {
        /// <summary>
        /// srcPath が srcRoot 配下にある場合のみ、destRoot 配下への差し替えパスを返す。
        /// 返す前に、移動先ディレクトリが無ければ作成する。
        /// 成功: true / 失敗: false（destPath は空）
        /// </summary>
        public static bool TryMapAndEnsureDirectory(string srcPath, string srcRoot, string destRoot, out string destPath)
        {
            destPath = string.Empty;
            if (string.IsNullOrWhiteSpace(srcPath) || string.IsNullOrWhiteSpace(srcRoot) || string.IsNullOrWhiteSpace(destRoot))
                return false;

            // 正規化（絶対パス化 + 末尾セパレータ統一）
            string srcFull = Path.GetFullPath(srcPath);
            string srcRootFull = AddTrailingSep(Path.GetFullPath(srcRoot));
            string destRootFull = AddTrailingSep(Path.GetFullPath(destRoot));

            // src が srcRoot 配下かを厳密に確認（先頭一致 + セパレータ境界）
            if (!srcFull.StartsWith(srcRootFull, StringComparison.OrdinalIgnoreCase))
                return false;

            // 相対パスを取得し、destRoot に差し替え
            string relative = Path.GetRelativePath(srcRootFull, srcFull);
            destPath = Path.GetFullPath(Path.Combine(destRootFull, relative));

            // フォルダが無ければ作成（既にある場合は何もしない）
            string? destDir = Path.GetDirectoryName(destPath);
            if (string.IsNullOrEmpty(destDir)) return false;
            Directory.CreateDirectory(destDir);

            return true;
        }

        private static string AddTrailingSep(string root)
        {
            if (string.IsNullOrEmpty(root)) return root;
            char sep = Path.DirectorySeparatorChar;
            return root.EndsWith(sep) ? root : root + sep;
        }
    }
}

