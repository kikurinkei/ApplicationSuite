using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// -------------------------------
// FILE: Static.LogFormatter.cs  （最小）
// -------------------------------

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Extract.Statics
{
    internal static class LogFormatter
    {
        // ひとまず未使用。将来、CSV風出力/サマリ等をここに集約。
        public static string OneLine(string level, string message)
            => $"{DateTime.Now:HH:mm:ss} [{level}] {message}";
    }
}