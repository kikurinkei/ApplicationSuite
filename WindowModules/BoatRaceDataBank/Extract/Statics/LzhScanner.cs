using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

// -------------------------------
// FILE: Static.LzhScanner.cs
// -------------------------------
namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Extract.Statics
{
    internal static class LzhScanner
    {
        public static IEnumerable<string> EnumerateLzhFiles(string lzhRoot, bool filterB, bool filterK)
        {
            if (!Directory.Exists(lzhRoot)) yield break;

            // すべての .lzh を再帰列挙
            var all = Directory.EnumerateFiles(lzhRoot, "*.lzh", SearchOption.AllDirectories);

            foreach (var f in all)
            {
                var label = PathMap.BranchLabelFromPath(f);
                if (label == "B" && !filterB) continue;
                if (label == "K" && !filterK) continue;
                yield return f;
            }
        }
    }
}
