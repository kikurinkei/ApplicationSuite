using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.UtilityTools
{
    public class Ui_Action
    {
        // UiAction.cs
        // 単一パラメータ（動詞）だけを扱う
        public enum UiAction
        {
            Paste,
            Process,
            Copy,
            ClearAll,
            AllInOne
        }
    }
}