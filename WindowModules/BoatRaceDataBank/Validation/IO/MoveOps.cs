using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Validation.Models;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Validation.IO
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
