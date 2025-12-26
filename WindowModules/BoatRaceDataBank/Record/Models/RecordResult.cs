using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Record.Models
{
    public sealed class RecordResult
    {
        public string FilePath { get; set; } = "";
        public string Result { get; set; } = "Ng"; // "Ok" or "Ng"
        public string Note { get; set; } = "";
        public string? DestinationPath { get; set; }
        public long ElapsedMs { get; set; }
    }
}
