using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// FILE: ExtractModels.cs
// 単一の“真実の場所”として Extract のドメイン型を集約
// namespace: ApplicationSuite.WindowModules.BoatRaceDataBank.Extract

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Extract
{
    // 上書きポリシー（既定＝Skip）
    public static class ExtractEnums
    {
        public enum OverwritePolicy { Skip, Overwrite, Rename }
    }

    // 失敗情報（必要最小限）
    public sealed class ExtractFailItem
    {
        public string LzhPath { get; }
        public string Reason { get; }
        public ExtractFailItem(string lzhPath, string reason)
        {
            LzhPath = lzhPath;
            Reason = reason;
        }
    }

    // 作業計画（LZH 1件 = TXT 1件の“ファイル単位”）
    public sealed class ExtractPlanItem
    {
        public string LzhPath { get; }
        public string TxtPath { get; }   // 期待する最終TXTのフルパス
        public string ArchPath { get; }   // 退避先LZHのフルパス
        public string WorkDir { get; }   // 一時展開先（空で作成→片付け）
        public long SizeBytes { get; }

        public ExtractPlanItem(string lzhPath, string txtPath, string archPath, string workDir, long sizeBytes)
        {
            LzhPath = lzhPath;
            TxtPath = txtPath;
            ArchPath = archPath;
            WorkDir = workDir;
            SizeBytes = sizeBytes;
        }
    }
}
