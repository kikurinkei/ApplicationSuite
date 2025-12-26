using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.AppGenerator.Catalog;

namespace ApplicationSuite.Runtime.Registries
{
    public static class ShellBitDictionaryRegistry
    {
        private static readonly Dictionary<string, Dictionary<string, UtilityMetaInfo>> _store = new();

        public static void Register(string windowUniqueId, Dictionary<string, UtilityMetaInfo> dict)
        {
            _store[windowUniqueId] = dict;
        }

        public static Dictionary<string, UtilityMetaInfo> GetAll(string windowUniqueId)
        {
            return _store.TryGetValue(windowUniqueId, out var d) ? d : new Dictionary<string, UtilityMetaInfo>();
        }
    }
}