using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.AppGenerator.Activation.Models;
using ApplicationSuite.Runtime.Registries._Base;

namespace ApplicationSuite.Runtime.Registries
{
    public class NavigationListRegistry : OneLevelRegistryBase<List<NavigationListItem>>
    {
        private static NavigationListRegistry? _instance;
        public static NavigationListRegistry Instance => _instance ??= new NavigationListRegistry();
        private NavigationListRegistry() { }
    }
}
