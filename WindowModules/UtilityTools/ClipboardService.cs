using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ApplicationSuite.WindowModules.UtilityTools
{
    public class ClipboardService : IClipboardService
    {
        public bool TryGetText(out string text)
        {
            if (Clipboard.ContainsText())
            {
                text = Clipboard.GetText();
                return true;
            }
            text = string.Empty;
            return false;
        }

        public void SetText(string text)
        {
            Clipboard.SetText(text ?? string.Empty);
        }
    }
}