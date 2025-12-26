using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.AppGenerator.Activation.Models;

namespace ApplicationSuite.Runtime.Registries
{
    public static class DashboardEntriesRegistry
    {
        private static readonly Dictionary<string, List<DashboardEntry>> _entries = new();

        public static void Register(string windowUniqueId, List<DashboardEntry> list)
        {
            _entries[windowUniqueId] = list;
        }

        public static List<DashboardEntry> Get(string windowUniqueId)
        {
            return _entries.TryGetValue(windowUniqueId, out var v) ? v : new List<DashboardEntry>();
        }
    }
}
