using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.Runtime.Registries._Base;

namespace ApplicationSuite.Runtime.Registries
{
    public class UserControlRegistry : TwoLevelRegistryBase<object>
    {
        private static UserControlRegistry? _instance;
        public static UserControlRegistry Instance => _instance ??= new UserControlRegistry();
        private UserControlRegistry() { }
    }
}