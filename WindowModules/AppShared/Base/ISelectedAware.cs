using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.AppShared.Base
{
    /// <summary>
    /// CompositeViewModelから通知を受け取るためのインターフェース。
    /// 表示対象に選ばれたとき、windowUniqueIdとelementIdを受け取ってログ取得などを行う。
    /// </summary>
    public interface ISelectedAware
    {
        /// <summary>
        /// 表示対象に選ばれたときに呼び出される通知メソッド。
        /// </summary>
        /// <param name="windowUniqueId">ウィンドウ識別子</param>
        /// <param name="elementId">要素識別子</param>
        void OnSelected(string windowUniqueId, string elementId);
    }
}
