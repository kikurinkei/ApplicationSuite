using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// -------------------------------
// FILE: Static.Extractor.cs
// -------------------------------
// Statics/Extractor.cs（全面置換）

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Extract.Statics
{
    internal static class Extractor
    {
        public static bool ExtractLzhToWorkAndPlaceTxt(string lzhPath, string workDir, string finalTxtPath, Action<string>? onInfo = null)
        {
            try
            {
                // a) WorkDirへ展開
                var (code, so, se) = SevenZipRunner.RunExtract(lzhPath, workDir);
                onInfo?.Invoke($"7Z: exit={code}");
                if (!string.IsNullOrWhiteSpace(se)) onInfo?.Invoke($"7Z-ERR: {TrimOneLine(se)}");
                if (!string.IsNullOrWhiteSpace(so)) onInfo?.Invoke($"7Z-OUT: {TakeFirstLine(so)}");
                if (code != 0) { SafeDeleteWorkDir(workDir); return false; }

                // b) WorkDir 内の .txt を発見
                var txts = Directory.EnumerateFiles(workDir, "*.txt", SearchOption.AllDirectories).ToArray();
                if (txts.Length == 0) { onInfo?.Invoke("NO-TXT: extracted none"); SafeDeleteWorkDir(workDir); return false; }

                // c) finalTxtPathへ移動（期待名で配置）※既存チェックは上流で済
                var src = PickBestTxt(txts, Path.GetFileNameWithoutExtension(finalTxtPath));
                IoSafe.EnsureDirectory(Path.GetDirectoryName(finalTxtPath)!);
                File.Move(src, finalTxtPath, overwrite: false);

                // d) WorkDir片付け
                SafeDeleteWorkDir(workDir);
                return true;
            }
            catch (Exception ex)
            {
                onInfo?.Invoke($"EXTRACT-ERROR: {ex.Message}");
                SafeDeleteWorkDir(workDir);
                return false;
            }
        }

        private static string PickBestTxt(string[] txts, string expectedStemUpper)
        {
            // 期待stem一致（大文字化比較）を最優先。なければ単一要素ならそれ。
            var match = txts.FirstOrDefault(p =>
                string.Equals(Path.GetFileNameWithoutExtension(p).ToUpperInvariant(), expectedStemUpper, StringComparison.Ordinal));
            if (match != null) return match;
            return txts.Length == 1 ? txts[0] : txts[0]; // 最小実装：複数時は先頭。必要なら将来厳密化
        }

        private static string TakeFirstLine(string s)
        {
            var idx = s.IndexOfAny(new[] { '\r', '\n' });
            return idx < 0 ? s : s[..idx];
        }
        private static string TrimOneLine(string s)
            => s.Replace("\r", " ").Replace("\n", " ").Trim();

        private static void SafeDeleteWorkDir(string workDir)
        {
            try { if (Directory.Exists(workDir)) Directory.Delete(workDir, recursive: true); }
            catch { /* noop */ }
        }
    }
}
