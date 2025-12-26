using System;

namespace ApplicationSuite.WindowModules.SecondaryWindow.ManualView.Services
{
    /// TextBoxを「文字オフセット位置」でスクロール＆キャレット移動させる Behavior。
    /// 
    /// 【役割】
    /// - VMは int (文字位置) を出すだけ
    /// - View(TextBox)側でスクロール実行（責務分離）
    /// 
    /// 【仕組み】
    /// - TargetCharIndex (int) の添付プロパティを定義
    /// - VMからこの値が変化すると OnTargetCharIndexChanged が呼ばれる
    /// - TextBox がまだロード前なら Loaded イベント後に実行
    /// - Dispatcher.BeginInvoke(Background) で ApplyScroll を実行
    /// - ApplyScroll:
    ///    - indexの位置にキャレットを置く
    ///    - その位置の行番号を取得し ScrollToLine() でスクロール
    /// 
    /// 【ポイント】
    /// - VMは「ScrollTargetIndexプロパティを更新するだけ」で良い
    /// - 実際のスクロールはUIスレッド側で確実に実行される
    public static class AnchorResolver
    {

        // ManualViewViewModel から呼ばれる。
        public static int ResolveFirstAnchorOffset(string content, string elementId)
        {
            if (string.IsNullOrEmpty(content)) return -1;
            if (string.IsNullOrEmpty(elementId)) return -1;

            string marker = "[[id:" + elementId + "]]";
            int length = content.Length;
            int pos = 0;

            while (pos < length)
            {
                int lineStart = pos;
                int lineEnd = pos;
                while (lineEnd < length && content[lineEnd] != '\n' && content[lineEnd] != '\r') lineEnd++;

                string line = content.Substring(lineStart, lineEnd - lineStart).Trim();
                if (string.Equals(line, marker, StringComparison.Ordinal)) return lineStart;

                if (lineEnd < length)
                {
                    if (content[lineEnd] == '\r' && lineEnd + 1 < length && content[lineEnd + 1] == '\n') pos = lineEnd + 2;
                    else pos = lineEnd + 1;
                }
                else
                {
                    pos = lineEnd;
                }
            }
            return -1;
        }
    }
}
