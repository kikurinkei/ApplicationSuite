using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;


// -------------------------------
// FILE: ExtractSession.cs
// -------------------------------

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Extract
{
    public sealed class ExtractSession
    {
        public int Total { get; set; }
        public int Ok { get; set; }
        public int Skip { get; set; }
        public int Fail { get; set; }

        public List<ExtractFailItem> Fails { get; } = new();
        public ExtractEnums.OverwritePolicy Overwrite { get; set; } = ExtractEnums.OverwritePolicy.Skip;
        public int DegreeOfParallelism { get; set; } = 1;

        public Stopwatch Stopwatch { get; } = Stopwatch.StartNew();
        // 将来: CancellationTokenSource Cts など
    }
}
