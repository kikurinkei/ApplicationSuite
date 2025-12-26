using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Record.Models;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Record.IO
{
    public static class MoveOps
    {
        public static IReadOnlyList<MoveResult> MoveOkFiles(
            IEnumerable<string> okFiles, string srcRoot, string destRoot)
        {
            var list = new List<MoveResult>();
            if (okFiles == null) return list;

            foreach (var src in okFiles)
            {
                var sw = Stopwatch.StartNew();
                var r = new MoveResult { FilePath = src, Result = "Ng" };

                try
                {
                    if (!PathMapper.TryMapAndEnsureDirectory(src, srcRoot, destRoot, out var dest))
                    {
                        r.Note = "Map failed";
                    }
                    else
                    {
                        r.DestinationPath = dest;
                        File.Move(src, dest);       // 上書きしない（存在時は例外→Ng）
                        r.Result = "Ok";
                    }
                }
                catch (Exception ex)
                {
                    r.Note = $"{ex.GetType().Name}: {ex.Message}";
                }
                finally
                {
                    sw.Stop();
                    r.ElapsedMs = sw.ElapsedMilliseconds;
                    list.Add(r);
                }
            }
            return list;
        }
    }
}
