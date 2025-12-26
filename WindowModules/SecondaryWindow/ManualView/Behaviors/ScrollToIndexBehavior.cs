using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ApplicationSuite.WindowModules.SecondaryWindow.ManualView.Behaviors
{
    /// コンテンツ文字列から「[[id:YourElementId]]」行を探すユーティリティ。
    /// 
    /// 【役割】
    /// - anchorId に一致するマーカー行を探す
    /// - 最初に見つかった行の「文字オフセット位置」を返す
    /// - 見つからなければ -1
    /// 
    /// 【仕様】
    /// - 行単独に [[id:xxx]] がある前提
    /// - 前後空白はOK
    /// - 大文字小文字は区別（Ordinal比較）
    /// 
    /// 【流れ】
    /// - contentを1行ずつ走査
    /// - line.Trim() == "[[id:elementId]]" なら lineStart位置を返す
    /// - 全部見てもなければ -1
    /// 
    /// 【ポイント】
    /// - 純粋な検索処理のみ（UIやVM依存なし）
    /// - VMが「anchorId受信」したら → AnchorResolverでoffset算出 → ScrollTargetIndex更新

    public static class ScrollToIndexBehavior
    {
        public static readonly DependencyProperty TargetCharIndexProperty =
            DependencyProperty.RegisterAttached(
                "TargetCharIndex",
                typeof(int),
                typeof(ScrollToIndexBehavior),
                new PropertyMetadata(-1, OnTargetCharIndexChanged));

        public static void SetTargetCharIndex(DependencyObject element, int value)
        {
            element.SetValue(TargetCharIndexProperty, value);
        }

        public static int GetTargetCharIndex(DependencyObject element)
        {
            return (int)element.GetValue(TargetCharIndexProperty);
        }

        private static void OnTargetCharIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = d as TextBox;
            if (textBox == null) return;

            int index = (int)e.NewValue;
            if (index < 0) return;

            if (!textBox.IsLoaded)
            {
                textBox.Loaded += (s, _) => ApplyScroll(textBox, index);
                return;
            }

            textBox.Dispatcher.BeginInvoke(
                (Action)(() => ApplyScroll(textBox, index)),
                DispatcherPriority.Background);
        }

        private static void ApplyScroll(TextBox textBox, int index)
        {
            int length = (textBox.Text == null) ? 0 : textBox.Text.Length;
            if (index < 0) index = 0;
            if (index > length) index = length;

            textBox.SelectionStart = index;
            textBox.SelectionLength = 0;
            textBox.CaretIndex = index;

            int lineIndex = 0;
            try
            {
                lineIndex = textBox.GetLineIndexFromCharacterIndex(index);
                if (lineIndex < 0) lineIndex = 0;
            }
            catch
            {
                lineIndex = 0;
            }
            textBox.ScrollToLine(lineIndex);
        }
    }
}
