using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

// -------------------------------
// FILE: Static.SevenZipRunner.cs
// -------------------------------

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Extract.Statics
{
    internal static class SevenZipRunner
    {
        // 初稿：ハードコード（要望どおり）
        private const string SevenZipExe = @"C:\Program Files\7-Zip\7z.exe";

        public static (int ExitCode, string StdOut, string StdErr) RunExtract(string lzhPath, string outputDir)
        {
            if (!File.Exists(SevenZipExe))
                throw new FileNotFoundException($"7z.exe が見つかりません: {SevenZipExe}");

            var psi = new ProcessStartInfo
            {
                FileName = SevenZipExe,
                Arguments = $"x -y -o\"{outputDir}\" \"{lzhPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var p = new Process { StartInfo = psi };
            var sbOut = new StringBuilder();
            var sbErr = new StringBuilder();

            p.OutputDataReceived += (_, e) => { if (e.Data != null) sbOut.AppendLine(e.Data); };
            p.ErrorDataReceived += (_, e) => { if (e.Data != null) sbErr.AppendLine(e.Data); };

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();

            return (p.ExitCode, sbOut.ToString(), sbErr.ToString());
        }
    }
}
