using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.Runtime.Registries._Base;
using ApplicationSuite.WindowModules.AppShared.Base;

namespace ApplicationSuite.Runtime.Registries
{
    public class CompositeViewModelRegistry : OneLevelRegistryBase<ICompositeViewModel>
    {
        private static CompositeViewModelRegistry? _instance;
        public static CompositeViewModelRegistry Instance => _instance ??= new CompositeViewModelRegistry();
        private CompositeViewModelRegistry() { }
    }
}
