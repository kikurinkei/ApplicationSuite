using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ApplicationSuite.WindowModules.AppShared.Base
{
        // Window継承はPrimaryBaseShell/SecondaryBaseShellで行う
    public abstract class AbstractBaseShell /*: Window */
    {
        public string? WindowUniqueId { get; set; }
        public bool? IsClosingInProgress { get; set; } = false;
        public bool? IsRestartRequested { get; set; } = false;

        protected AbstractBaseShell() { }
    }
}