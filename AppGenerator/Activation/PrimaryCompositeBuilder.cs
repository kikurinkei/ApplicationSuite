using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.AppGenerator.Catalog;
using ApplicationSuite.Runtime.Registries;
using ApplicationSuite.WindowModules.AppShared.Base;

namespace ApplicationSuite.AppGenerator.Activation
{
    /// <summary>
    /// PrimaryCompositeBuilder の責務は、ViewModelRegistry から ViewModel を取得し、
    /// PrimaryCompositeViewModel に注入して構築・登録することです。
    /// </summary>
    public static class PrimaryCompositeBuilder
    {
        /// <summary>
        /// 指定された elementIds に従って ViewModel を注入した CompositeViewModel を構築します。
        /// </summary>
        //public static CompositeViewModel Build(string windowUniqueId, List<string> elementIds)
        public static PrimaryCompositeViewModel BuildAndRegister(
            string windowUniqueId, Dictionary<string, UtilityMetaInfo> childIds)
        {
            var PrimaryCompositeViewModel = new PrimaryCompositeViewModel();

            //foreach (var id in elementIds)
            foreach (var kv in childIds)
            {
                var vm = ViewModelRegistry.Instance.Get(windowUniqueId, kv.Value.UIElementId);
                if (vm != null)
                {
                    PrimaryCompositeViewModel.Inject(kv.Value.UIElementId, vm);
                    Console.WriteLine($"[AutoCompositeBuilder][OK] Inject成功: {kv.Value.UIElementId}");
                }
                else
                {
                    Console.WriteLine($"[AutoCompositeBuilder][Skip] ViewModel 見つかりません: {kv.Value.UIElementId}");
                }
            }

            CompositeViewModelRegistry.Instance.Register(windowUniqueId, PrimaryCompositeViewModel as ICompositeViewModel);
            Console.WriteLine("[AutoCompositeBuilder][OK] CompositeViewModel 登録成功");
            return PrimaryCompositeViewModel;
        }
    }
}
