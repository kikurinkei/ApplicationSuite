using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Extract.Statics;

// -------------------------------
// FILE: Static.Archiver.cs
// -------------------------------
namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Extract.Statics
{
    internal static class Archiver
    {
        public static bool MoveToArchive(string srcLzh, string dstArchPath, Action<string>? onInfo = null)
        {
            try
            {
                IoSafe.EnsureDirectory(Path.GetDirectoryName(dstArchPath)!);

                // 既存あれば上書きせず一旦削除（ARCH側は同名再発の可能性低いが安全に）
                if (File.Exists(dstArchPath))
                {
                    File.Delete(dstArchPath);
                }
                File.Move(srcLzh, dstArchPath);
                onInfo?.Invoke($"ARCH: {srcLzh} -> {dstArchPath}");
                return true;
            }
            catch (Exception ex)
            {
                onInfo?.Invoke($"ARCH-ERROR: {ex.Message}");
                return false;
            }
        }
    }
}
