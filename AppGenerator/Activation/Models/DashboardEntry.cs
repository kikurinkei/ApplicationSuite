using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.AppGenerator.Activation.Models
{
    public sealed class DashboardEntry
    {
        public string ElementId { get; set; } = string.Empty;
        public string ElementName { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Usage { get; set; } = string.Empty;
    }

}
