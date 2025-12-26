using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.UtilityTools
{
    public interface IClipboardService
    {
        bool TryGetText(out string text);
        void SetText(string text);

        // TODO: 設定変更差し込み口（例：Encoding）
        // private Encoding _clipboardEncoding = Encoding.UTF8;
        // public void SetEncoding(Encoding encoding) => _clipboardEncoding = encoding;
    }
}