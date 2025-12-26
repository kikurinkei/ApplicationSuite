using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.AppShared.Base
{
    public interface IShell
    {
        string? WindowUniqueId { get; set; }
        bool? IsClosingInProgress { get; set; }
        bool? IsRestartRequested { get; set; }

        void Show();                     // 表示処理（ShowDialogでも可）
        void Close();                    // クローズ処理

    }   


    /*
    public interface IShell
    {
        string? WindowUniqueId { get; set; }
        bool? IsClosingInProgress { get; set; }
        bool? IsRestartRequested { get; set; }
    }
*/
}
