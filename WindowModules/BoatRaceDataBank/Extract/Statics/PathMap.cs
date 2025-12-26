using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

// -------------------------------
// FILE: Static.PathMap.cs
// -------------------------------
// Statics/PathMap.cs（全面置換）

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Extract.Statics
{
    internal static class PathMap
    {
        public static (string LzhPath, string TxtPath, string ArchPath, string WorkDir) MapPaths(
            string lzhRoot, string txtRoot, string archRoot, string lzhFullPath)
        {
            var rel = GetRelativePath(lzhRoot, lzhFullPath);          // 例: "B\\2025\\b250903.lzh"
            var relDir = Path.GetDirectoryName(rel) ?? string.Empty;  // "B\\2025"
            var lzhFile = Path.GetFileName(lzhFullPath);              // "b250903.lzh"
            var stem = Path.GetFileNameWithoutExtension(lzhFile);     // "b250903"

            // 期待TXT名：大文字 + ".TXT"
            var expectedTxtName = ToExpectedTxtName(stem);            // "B250903.TXT"
            var txtPath = Path.Combine(txtRoot, relDir, expectedTxtName);

            var archPath = Path.Combine(archRoot, relDir, lzhFile);

            // 一時展開先：TXT\_work\<relDir>\<stem>\
            var workDir = Path.Combine(txtRoot, "_work", relDir, stem);

            return (lzhFullPath, txtPath, archPath, workDir);
        }

        public static string BranchLabelFromPath(string lzhFullPath)
        {
            var parts = lzhFullPath.Replace('/', '\\').Split('\\', StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
                if (string.Equals(p, "B", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p, "K", StringComparison.OrdinalIgnoreCase))
                    return p.ToUpperInvariant();
            // ファイル名先頭が B/K の場合も拾う
            var fn = Path.GetFileName(lzhFullPath);
            if (!string.IsNullOrEmpty(fn) && (fn[0] == 'B' || fn[0] == 'b' || fn[0] == 'K' || fn[0] == 'k'))
                return char.ToUpperInvariant(fn[0]).ToString();
            return "-";
        }

        public static string ToExpectedTxtName(string stem)
            => (stem ?? string.Empty).ToUpperInvariant() + ".TXT";

        private static string GetRelativePath(string root, string fullPath)
        {
            var rootNorm = EnsureSepEnd(Path.GetFullPath(root));
            var fullNorm = Path.GetFullPath(fullPath);
            if (!fullNorm.StartsWith(rootNorm, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"パスがルート配下ではありません: {fullPath}");
            return fullNorm.Substring(rootNorm.Length);
        }

        private static string EnsureSepEnd(string p)
            => p.EndsWith(Path.DirectorySeparatorChar) ? p : p + Path.DirectorySeparatorChar;
    }
}
