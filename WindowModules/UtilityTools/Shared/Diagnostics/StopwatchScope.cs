using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

/*
SUMMARY:
- 計測の薄いスコープラッパ（初稿ではログ出力はせず、将来のために置くだけ）。
*/


namespace ApplicationSuite.WindowModules.UtilityTools.Shared.Diagnostics
{
    public readonly struct StopwatchScope : IDisposable
    {
        private readonly Stopwatch _sw;
        private readonly Action<TimeSpan>? _onDispose;

        public StopwatchScope(Action<TimeSpan>? onDispose)
        {
            _onDispose = onDispose;
            _sw = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _sw.Stop();
            _onDispose?.Invoke(_sw.Elapsed);
        }
    }
}
