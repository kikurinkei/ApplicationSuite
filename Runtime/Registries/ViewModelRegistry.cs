using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.Runtime.Registries._Base;
using ApplicationSuite.WindowModules.AppShared.Base;

namespace ApplicationSuite.Runtime.Registries
{
    public class ViewModelRegistry : TwoLevelRegistryBase<BaseViewModel>
    {
        private static ViewModelRegistry? _instance;
        public static ViewModelRegistry Instance => _instance ??= new ViewModelRegistry();
        private ViewModelRegistry() { }
    }
}