using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Validation.Models
{
    public sealed class MoveResult
    {
        public string FilePath { get; set; } = string.Empty;
        public string? DestinationPath { get; set; }
        public string Result { get; set; } = "Ok";   // "Ok" or "Ng"
        public string? Note { get; set; }
        public long ElapsedMs { get; set; }
    }
}