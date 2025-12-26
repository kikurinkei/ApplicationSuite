using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// -------------------------------
// FILE: Static.IoSafe.cs
// -------------------------------
// Statics/IoSafe.cs（追記）
namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Extract.Statics
{
    internal static class IoSafe
    {
        public static void EnsureDirectory(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir)) return;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }

        public static void RecreateEmptyDirectory(string dir) // ★追加
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, recursive: true);
            }
            Directory.CreateDirectory(dir);
        }

        public static long GetFileSizeOrZero(string path)
        {
            try { var fi = new FileInfo(path); return fi.Exists ? fi.Length : 0L; }
            catch { return 0L; }
        }
    }
}
