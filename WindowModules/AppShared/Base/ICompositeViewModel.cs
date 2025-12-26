using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.AppShared.Base
{
    /// <summary>
    /// CompositeViewModel の共通操作を定義するインターフェース
    /// </summary>
    public interface ICompositeViewModel
    {
        public string? PairedWindowUniqueId { get; set; } // null許容型にした。

        public string? WindowUniqueId { get; set; } // null許容型にした。
        
        public bool? IsRealtimeSyncEnabled { get; set; } // null許容型にした。

        public string? CCVM { get; set; } // null許容型にした。


        void Inject(string elementId, BaseViewModel viewModel);
        BaseViewModel? Resolve(string elementId);

        IReadOnlyDictionary<string, BaseViewModel> AVM { get; }

        /// <summary>
        /// 表示中の ViewModel（View 側が ContentControl でバインドする用）
        /// </summary>
        BaseViewModel? CurrentContentViewModel { get; set; }
    }
}
