using ApplicationSuite.Runtime.Service.Routing;
using ApplicationSuite.WindowModules.SecondaryWindow.ManualView.Registry;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.Runtime.Windowing.Close
{
    public class RestartRequestProc
    {
        public static void Process(string status, string shellId)
        {
            ShutdownRequestProc.Execute(status);

            PrimaryShellLifecycleController.HandleWindowLifecycle("OPEN", shellId, "", null, null);
        }
    }
}

