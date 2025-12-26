using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.Runtime.Service.Routing
{
    ///// <summary>
    ///// ユニークなウィンドウIDを生成する。
    ///// </summary>
    public static class GenerateWindowId
    {
        private static int _count = 0;
        public static string GetNextNumber()
        {
            _count++;
            return _count.ToString("D5"); // 5桁のゼロパディング
        }
        /// <summary>
        /// shellIdを元にユニークなウィンドウIDを生成する。
        /// </summary>
        public static string GetWindowUniqueId(string shellId)
        {
            return $"{shellId}{GetNextNumber()}";
        }

    }
}
