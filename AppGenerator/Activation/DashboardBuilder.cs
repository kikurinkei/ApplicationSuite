using ApplicationSuite.AppGenerator.Activation.Models;
using ApplicationSuite.AppGenerator.Catalog;
using ApplicationSuite.Runtime.Registries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.AppGenerator.Activation
{
    public class DashboardBuilder
    {
        //Build(string windowUniqueId, Dictionary<string, UtilityMetaInfo> childIds)
        public static void BuildAndRegister(string windowUniqueId, Dictionary<string, UtilityMetaInfo> dashDict)
        {
            //var baseDict = ShellBitDictionaryRegistry.GetAll(windowUniqueId);
            //var navDict = WindowApplicationSuite.Core.Builders.BitFilter.FilterByTypeFlag(baseDict, 16);

            var list = new List<DashboardEntry>();
            foreach (var kv in dashDict)
            {
                var meta = kv.Value;
                list.Add(new DashboardEntry
                {
                    ElementId = meta.UIElementId,
                    ElementName = meta.UIElementName,
                    IconPath = meta.UIIconPath,
                    Description = meta.UIDescription,
                    Usage = meta.UIUsage
                });
            }

            DashboardEntriesRegistry.Register(windowUniqueId, list);
        }
    }
}