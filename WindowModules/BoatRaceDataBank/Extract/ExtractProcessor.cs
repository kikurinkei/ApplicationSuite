using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Extract.Statics;
// -------------------------------
// FILE: ExtractProcessor.cs
// -------------------------------// ExtractProcessor.cs（差分）
namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Extract
{
    public sealed class ExtractProcessor
    {
        private const string DirLzh = "LZH";
        private const string DirTxt = "TXT";
        private const string DirArch = "ARCH";

        public IReadOnlyList<ExtractPlanItem> Scan(string rootPath, bool filterB, bool filterK)
        {
            if (string.IsNullOrWhiteSpace(rootPath)) throw new InvalidOperationException("RootPath が空です。");

            var lzhRoot = Path.Combine(rootPath, DirLzh);
            var txtRoot = Path.Combine(rootPath, DirTxt);
            var archRoot = Path.Combine(rootPath, DirArch);

            IoSafe.EnsureDirectory(txtRoot);
            IoSafe.EnsureDirectory(archRoot);

            var lzhs = LzhScanner.EnumerateLzhFiles(lzhRoot, filterB, filterK);
            var plans = new List<ExtractPlanItem>(capacity: 128);

            foreach (var lzh in lzhs)
            {
                var map = PathMap.MapPaths(lzhRoot, txtRoot, archRoot, lzh);
                var size = IoSafe.GetFileSizeOrZero(lzh);
                plans.Add(new ExtractPlanItem(map.LzhPath, map.TxtPath, map.ArchPath, map.WorkDir, size));
            }
            return plans;
        }

        public void Run(
            IReadOnlyList<ExtractPlanItem> plans,
            ExtractSession session,
            Action<string> onInfo,
            Action<string> onCurrent,
            CancellationToken ct)
        {
            session.Total = plans.Count;
            onInfo($"RUN START: {plans.Count} files");

            foreach (var p in plans)
            {
                if (ct.IsCancellationRequested) break;
                var label = Statics.PathMap.BranchLabelFromPath(p.LzhPath);
                onCurrent(Path.GetFileName(p.LzhPath));

                try
                {
                    // 1) 期待TXTが既にあるなら：解凍せずに LZH→ARCH だけ行う
                    if (File.Exists(p.TxtPath))
                    {
                        IoSafe.EnsureDirectory(Path.GetDirectoryName(p.ArchPath)!);
                        if (Archiver.MoveToArchive(p.LzhPath, p.ArchPath, onInfo))
                        {
                            session.Skip++;
                            onInfo($"SKIP (exists): {label} {p.TxtPath} → ARCH only");
                        }
                        else
                        {
                            session.Fail++;
                            onInfo($"FAIL: {label} {p.LzhPath} (archive move error)");
                        }
                        continue;
                    }

                    // 2) 解凍：WorkDirへきれいに展開 → 期待TXT名で配置
                    IoSafe.EnsureDirectory(Path.GetDirectoryName(p.TxtPath)!);
                    IoSafe.RecreateEmptyDirectory(p.WorkDir);

                    var ok = Extractor.ExtractLzhToWorkAndPlaceTxt(p.LzhPath, p.WorkDir, p.TxtPath, onInfo);
                    // WorkDirはExtractor内で片付ける（成功/失敗いずれも）

                    if (!ok)
                    {
                        session.Fail++;
                        onInfo($"FAIL: {label} {p.LzhPath} (extract/place error)");
                        continue;
                    }

                    // 3) ARCHへ原本移動
                    IoSafe.EnsureDirectory(Path.GetDirectoryName(p.ArchPath)!);
                    var moved = Archiver.MoveToArchive(p.LzhPath, p.ArchPath, onInfo);
                    if (moved)
                    {
                        session.Ok++;
                        onInfo($"OK: {label} {p.LzhPath} → {p.TxtPath} / ARCH");
                    }
                    else
                    {
                        session.Fail++;
                        onInfo($"FAIL: {label} {p.LzhPath} (archive move error)");
                    }
                }
                catch (Exception ex)
                {
                    session.Fail++;
                    onInfo($"ERROR: {label} {p.LzhPath} - {ex.Message}");
                }
            }

            onInfo($"RUN END: OK={session.Ok} SKIP={session.Skip} FAIL={session.Fail}");
        }
    }
}
