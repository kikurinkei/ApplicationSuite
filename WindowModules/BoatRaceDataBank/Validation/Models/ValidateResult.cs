using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Validation.Models
{
    public sealed class ValidateResult
    {
        public string FilePath { get; set; } = "";
        public string Result { get; set; } = "Ng"; // "Ok" or "Ng"
        public string Note { get; set; } = "";
        public string? DestinationPath { get; set; }
        public long ElapsedMs { get; set; }
    }
}
